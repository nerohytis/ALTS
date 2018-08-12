using System;
using System.Collections.Generic;
using System.Linq;
using PCSC;
using PCSC.Exceptions;
using PCSC.Iso7816;

namespace EmvLib
{
    public class SmartCard:IDisposable
    {
        public List<SmartApplication> Applications { get; set; } = new List<SmartApplication>();
        public IsoReader reader;
        ISCardContext _context;

        public SmartCard(string readerName)
        {
            var contextFactory = ContextFactory.Instance;
            _context = contextFactory.Establish(SCardScope.System);
            if (string.IsNullOrEmpty(readerName))
            {
                throw new ApplicationException("No smartCard readers found");
            }

            reader = new IsoReader(_context, readerName, SCardShareMode.Exclusive, SCardProtocol.Any, true);

        }

        public SmartCard(SCardContext context, string readerName)
        {
            reader = new IsoReader(context);
            reader.Connect(readerName, SCardShareMode.Exclusive, SCardProtocol.Any);
            

        }
        public SmartCard(IsoReader reader)
        {
            this.reader = reader;
        }

        public void GetSingleApplication(byte[] aid)
        {

            CommandApdu apdu = new CommandApdu(IsoCase.Case4Short, reader.ActiveProtocol);
            apdu.CLA = new ClassByte(ClaHighPart.Iso0x, SecureMessagingFormat.None, 0);
            apdu.Instruction = InstructionCode.SelectFile;
            apdu.P1 = 0x04; //select by name
            apdu.P2 = 00; // First or only occurrence
            apdu.Data = aid;
            System.Diagnostics.Debug.WriteLine(StringTools.ByteArrayToHexString(apdu.ToArray()));
            Response res = reader.Transmit(
                apdu);

            
            if (res.SW1 == 0x90)
            {
                Applications.Add(new SmartApplication(res.GetData(), reader));
            }
            else
            {
                throw new PCSCException(SCardError.FileNotFound, "Select command failed");
            }
        }

        public int GetAIDs(bool ReturnOnFirst=false)
        {
            
            CommandApdu apdu = new CommandApdu(IsoCase.Case4Short, reader.ActiveProtocol);
            apdu.CLA = new ClassByte(ClaHighPart.Iso0x, SecureMessagingFormat.None, 0);
            apdu.Instruction = InstructionCode.SelectFile;
            apdu.P1 = 0x04; //select by name
            apdu.P2 = 00;   // First or only occurrence
            //apdu.Le = 0;
            foreach (var app in EmvConstants.AID_LIST_BYTES)
            {
                byte[] aid = (app);
                apdu.Data = aid;
                var res = reader.Transmit(apdu); // data buffer
                if (res.SW1 == 0x90)
                {
                    Applications.Add(new SmartApplication(res.GetData(),reader));
                    if (ReturnOnFirst)
                    {
                        return 1;
                    }
                }
            }
            return Applications.Count;
        }

        public void Dispose()
        {
            reader.Disconnect(SCardReaderDisposition.Unpower);
            //reader.CurrentContext.Release();
            //reader.CurrentContext.Dispose();



        }
    }

}

