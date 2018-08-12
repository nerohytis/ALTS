using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmvLib
{

    public class AflResult
    {
        public List<AflEntry> AflEntries=new List<AflEntry>();
    }

    public class AflEntry
    {
        public int Sfi;
        public int StartRecord;
        public int EndRecord;
        public int OfflineRecords;
    }
    public class TlvTools
    {


        public static AflResult AflParser(byte[] afl)
        {
            AflResult res = new AflResult();
            int i = 0;
            while (i < afl.Length)
            {
                AflEntry entry = new AflEntry();
                entry.Sfi = afl[i++] >> 3;
                entry.StartRecord = afl[i++];
                entry.EndRecord = afl[i++];
                entry.OfflineRecords= afl[i++];
                res.AflEntries.Add(entry);
            }
            return res;
        }

        public static byte[] parseTagLengthData(byte[] data)
        {
            int index = 0;
            List<byte> _tagList = new List<byte>();
            while (index < data.Length)
            {
                var temptag = new List<byte>();

                //Get the tag name
                temptag.Add(data[index]);
                if ((data[index] & EmvConstants.SeeSubsequentBytes) == EmvConstants.SeeSubsequentBytes)
                {
                    index++;
                    temptag.Add(data[index]);
                }
                Console.WriteLine("EmvTag " + StringTools.ByteArrayToHexString(temptag.ToArray()));
                index++;

                // Get the length of the data to follow

                if ((data[index] & 0x80) == 0x80)
                {
                    int bytesForLenght = data[index] % 0x80;
                    index++;
                    for (int i = 0; i < bytesForLenght; i++)
                    {
                        index++;
                    }
                }
                else
                {
                    index++;
                }
                string tagname=StringTools.ByteArrayToHexString(temptag.ToArray()).ToUpper();
                if (EmvConstants.PdolTags.ContainsKey(tagname))
                {
                    _tagList.AddRange(StringTools.HexStringToByteArray(EmvConstants.PdolTags[tagname]));
                }
                else
                {
                    throw new Exception($"Unknown PDOL tag {tagname}");
                }
            }
            return _tagList.ToArray();
        }

    }
}
