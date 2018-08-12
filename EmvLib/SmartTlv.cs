using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmvLib
{
    public class SmartTlv
    {
        byte[] tlvdata { get; set; }
        public List<SmartTag> TagTree { get; }

        public List<SmartTag> TagList { get; } = new List<SmartTag>();
        public SmartTlv(byte[] tlvdata)
        {
            this.tlvdata = tlvdata;
            TagTree = parsetlv(tlvdata);
        }

        public void dump(ref StringBuilder dumpStr, int ident = 0)
        {
            _dump(TagTree, ref dumpStr, ident);
        }
        private static void _dump(List<SmartTag> taglist,ref StringBuilder dumpStr, int ident = 0)
        {
            foreach (SmartTag tempTag in taglist)
            {
                dumpStr.Append('\t', ident);
                dumpStr.AppendLine($"{tempTag.TagStringName} - {tempTag.Description} : {tempTag.TlvData} {tempTag.HasChildren} ");
                if (tempTag.IsConstructed)
                {
                    _dump(tempTag.Children, ref dumpStr, ++ident);
                }

            }
        }

        internal List<SmartTag> parsetlv(byte[] data,SmartTag parent=null)
        {
            List<SmartTag> tags = new List<SmartTag>();
            int index = 0;
            while (index < data.Length)
            {
                var temptag = new List<byte>();
                var tagValue = new List<byte>();
                var tagLen = 0;

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
                        tagLen += data[index];
                        index++;
                    }
                }
                else
                {
                    tagLen = data[index];
                    index++;
                }

                // Get the value of the tag
                for (int i = 0; i < tagLen; i++)
                {
                    try
                    {
                        tagValue.Add(data[index]);
                        index++;
                    }
                    catch (Exception )
                    {
                        i = tagLen;
                    }
                }
                string tagDesc = EmvConstants.getTagDescription(StringTools.ByteArrayToHexString(temptag.ToArray()).ToLower());
                SmartTag smarttag = new SmartTag(temptag, tagLen, tagValue, tagDesc,parent);
                
                tags.Add(smarttag);
                if (smarttag.IsConstructed)
                {
                    smarttag.Children= parsetlv(smarttag.TagValue.ToArray(),smarttag);
                }
                TagList.Add(smarttag);
            }
            return tags;
        }
    }

    public class SmartTag
    {

        public List<byte> Tagname { get; }

        public string TagStringName => StringTools.ByteArrayToHexString(Tagname.ToArray());
        public string TlvData => StringTools.ByteArrayToHexString(TagValue.ToArray());
        public int DataLen { get; }
        public List<byte> TagValue { get; }
        public string Description { get; }
        public bool HasParent => Parent != null;
        public bool HasChildren => Children != null && Children.Any(); 

        public SmartTag Parent { get; set; }
        public List<SmartTag> Children { get; set; }

        public bool IsConstructed => (Tagname[0] & EmvConstants.ConstructedDataObject) == EmvConstants.ConstructedDataObject;

        public SmartTag(List<byte> tagname, int dataLen, List<byte> tagValue, string tagDescript,SmartTag parent=null)
        {
            Tagname = tagname;
            DataLen = dataLen;
            TagValue = tagValue;
            Description = tagDescript;
            Parent = parent;
        }
    }
}
