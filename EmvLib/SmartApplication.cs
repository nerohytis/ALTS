using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PCSC;
using PCSC.Exceptions;
using PCSC.Iso7816;

namespace EmvLib
{
    /// <summary>
    /// Object representation of Smart card Application
    /// </summary>
    public class SmartApplication
    {
        /// <summary>
        /// The smart card reader
        /// </summary>
        private readonly IsoReader _reader;

        /// <summary>
        /// Constractor for the SmartApplication object
        /// </summary>
        /// <param name="selectresponce">The reponce of the card following the SELECT application command </param>
        /// <param name="reader">The IsoReader communicating with the application/card </param>
        public SmartApplication(byte[] selectresponce, IsoReader reader)
        {
            _reader = reader;
            RES_SELECT = selectresponce;
            TLV_SELECT = new SmartTlv(selectresponce);
            AID = TLV_SELECT.TagList.Single(t => t.TagStringName == "84").TagValue.ToArray();
        }

        /// <summary>
        /// Prepares the Get processing options command
        /// </summary>
        /// <param name="gpodata"></param>
        public void SetGpo(byte[] gpodata)
        {
            RES_GPO = gpodata;
            TLV_GPO = new SmartTlv(gpodata);
        }

        /// <summary>
        /// The application Identifier in bytes
        /// </summary>
        public byte[] AID { get; private set; } 
        /// <summary>
        /// The reponce of the select command
        /// </summary>
        public byte[] RES_SELECT { get; set; }
        /// <summary>
        /// The reponce of the GPO command
        /// </summary>
        public byte[] RES_GPO { get; set; }
        /// <summary>
        /// The reponce of the select command in Tag-Length-Value format
        /// </summary>
        public SmartTlv TLV_SELECT { get;}
        /// <summary>
        /// The reponce of the GPO command in Tag-Length-Value format
        /// </summary>
        public SmartTlv TLV_GPO { get; private set; }

        /// <summary>
        /// List of Emv records in the card
        /// </summary>
        public List<SmartEmvRecord> EmvRecords { get; private set; }

        /// <summary>
        /// The Response Message Template Format of the GPO responce. 
        /// </summary>
        public EmvConstants.GpoTemplateFormat GpoTemplateFormat { get; private set; }

        
        /// <summary>
        /// Issues a select Application command
        /// </summary>
        public void SelectApplication()
        {
            //_reader.Reconnect(SCardShareMode.Exclusive, SCardProtocol.Any, SCardReaderDisposition.Eject);
            CommandApdu apdu = new CommandApdu(IsoCase.Case4Short, _reader.ActiveProtocol)
            {
                CLA = new ClassByte(ClaHighPart.Iso0x, SecureMessagingFormat.None, 0),
                Instruction = InstructionCode.SelectFile,
                P1 = 0x04,
                P2 = 00,
                Data = AID
            };

            var res = _reader.TransmitWithLog(apdu);
        }

        /// <summary>
        /// Issues a GPO command. 
        /// </summary>
        /// <remarks>Pdol data , reponce parsing and storing are handled internaly</remarks>
        public void GetProcessingOptions()
        {
            List<byte> pdoldata=new List<byte> {0x83};
            var pdol=TLV_SELECT.TagList.SingleOrDefault(t => t.TagStringName == "9F38");
            if (pdol != null)
            {
                var tempdata = TlvTools.parseTagLengthData(pdol.TagValue.ToArray());
                pdoldata.Add((byte) tempdata.Length);
                pdoldata.AddRange(tempdata);
            }
            else
            {
                pdoldata.Add(0x00);
            }

            CommandApdu apdu = new CommandApdu(IsoCase.Case4Short, _reader.ActiveProtocol)
            {
                CLA = new ClassByte(ClaHighPart.Iso8x, SecureMessagingFormat.None, 0),
                INS = 0xA8,
                P1 = 0x00,
                P2 = 00,
                Data = pdoldata.ToArray()
            };

            var res = _reader.TransmitWithLog(apdu); // data buffer
            

            RES_GPO = res.GetData();
            GpoTemplateFormat = (RES_GPO[0] == (byte) EmvConstants.GpoTemplateFormat.Format1)
                ? EmvConstants.GpoTemplateFormat.Format1
                : EmvConstants.GpoTemplateFormat.Format2;
            TLV_GPO = new SmartTlv(RES_GPO);
        }

        /// <summary>
        /// Issue a read-record command for all records included in the GPO Responce, Tag AFL(Application file locator)
        /// </summary>
        public void ReadRecords()
        {
            byte[] afl;
            if (GpoTemplateFormat == EmvConstants.GpoTemplateFormat.Format1)
            {
                List<byte>gpores = TLV_GPO.TagList.Single(t => t.TagStringName == "80").TagValue;
                afl = gpores.Skip(2).ToArray();
            }
            else
            {
                afl = TLV_GPO.TagList.SingleOrDefault(t => t.TagStringName == "94")?.TagValue.ToArray();
                if (afl == null)
                {
                    return;
                }
            }

            AflResult aflpos= TlvTools.AflParser(afl);
            EmvRecords = new List<SmartEmvRecord>();
            foreach (var entry in aflpos.AflEntries)
            {
                for (int irecord= entry.StartRecord;irecord<=entry.EndRecord;irecord++ )
                //foreach (int irecord in Enumerable.Range(entry.StartRecord, entry.EndRecord))
                {
                    CommandApdu apdu = new CommandApdu(IsoCase.Case2Short, _reader.ActiveProtocol);
                    apdu.CLA = 0x00;
                    apdu.INS = 0xB2; //GPO
                    apdu.P1 =(byte) irecord; //select by name
                    apdu.P2 = (byte) ((entry.Sfi << 3) | 4); // First or only occurrence
                    
                    var res = _reader.TransmitWithLog(apdu); // data buffer
                    
                    if (res.SW1 == 0x6C)
                    {
                        apdu.Le = res.SW2;
                        res = _reader.TransmitWithLog(apdu); // data buffer
                    }

                    if (res.SW1 != (byte)SW1Code.Normal)
                    {
                        throw new PCSCException(SCardError.CardUnsupported, "GPO not fully supported");
                    }
                    var record = new SmartEmvRecord(entry.Sfi,irecord,(irecord<=entry.OfflineRecords), res.GetData());
                    EmvRecords.Add(record);

                }
            }
        }

        /// <summary>
        /// Retreive the value of the gigen tag
        /// </summary>
        /// <param name="type">Where to look in the card data</param>
        /// <param name="tagname">The tag name</param>
        /// <returns>The tag data</returns>
        public string GetTagValue(EmvConstants.ResponceType type, string tagname)
        {
            string res = string.Empty;
            switch (type)
            {
                case EmvConstants.ResponceType.All:
                    res = _GetTagValue(EmvConstants.ResponceType.ReaderRecord, tagname);
                    if (string.IsNullOrEmpty(res))
                    {
                        res = _GetTagValue(EmvConstants.ResponceType.Gpo, tagname);
                    }
                    return res;
                case EmvConstants.ResponceType.Select:
                    throw new NotImplementedException("EMV.ResponceType.Select not implemented");
                default:
                    return _GetTagValue(type, tagname);

            }
        }


        /// <summary>
        /// Handles the retreival of the GetTagValue
        /// </summary>
        /// <see cref="GetTagValue(EmvConstants.ResponceType, string)"/>
        /// <param name="type"></param>
        /// <param name="tagname"></param>
        /// <returns></returns>
        private  string _GetTagValue(EmvConstants.ResponceType type,string tagname)
        {
            string res=null;
            switch (type)
            {
                case EmvConstants.ResponceType.ReaderRecord:
                    try
                    {
                        if (EmvRecords == null)
                        {
                            return null;
                        }
                        res = (from r in EmvRecords
                            from t in r.TLV_READRECORD.TagList
                            where t?.TagStringName.ToUpper() == tagname.ToUpper()
                            select t?.TlvData).SingleOrDefault();
                    }
                    catch (InvalidOperationException)
                    {
                        throw new Exception($"The smartcard has many tags with name {tagname}");
                    }
                    break;
                case EmvConstants.ResponceType.Gpo:
                    res=(from t in TLV_GPO.TagList where string.Equals(t.TagStringName, tagname, StringComparison.CurrentCultureIgnoreCase) select t.TlvData)
                        .SingleOrDefault();
                    break;
                case EmvConstants.ResponceType.Select:
                    throw new NotImplementedException("EMV.ResponceType.Select");
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            return res;
        }

        /// <summary>
        /// Retreive the values used for the Offline Auth
        /// </summary>
        /// <returns>The values concatenated. Sda tag list included, if any</returns>
        public string GetOfflineTagValues()
        {
            var offlineRecords = this.EmvRecords.Where(r => r.IsOffline);
            string offlineTagsConcat = "";
            foreach (var record in offlineRecords)
            {
                foreach (var tag in record.TLV_READRECORD.TagList)
                {
                    offlineTagsConcat += tag.TlvData;
                }
            }
            var sdaTagList = GetTagValue(EmvConstants.ResponceType.ReaderRecord, "9F4A");
            if (!string.IsNullOrEmpty(sdaTagList))
            {
                offlineTagsConcat += GetTagValue(EmvConstants.ResponceType.ReaderRecord, sdaTagList);
            }


            return offlineTagsConcat;
        }

        /// <summary>
        /// String representation of the class
        /// </summary>
        /// <returns>The representation within a StringBuilder class</returns>
        public StringBuilder Dump()
        {
            StringBuilder sb = new StringBuilder();
            this.TLV_GPO.dump(ref sb);
            this.TLV_SELECT.dump(ref sb);
            foreach (var rc in this.EmvRecords)
            {
                rc.TLV_READRECORD.dump(ref sb);
            }

            return sb;
        }
    }
}
