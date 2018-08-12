using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmvLib
{
    public class SmartEmvRecord
    {
        public int Sfi { get;  }
        public int Record { get;  }
        public bool IsOffline { get; }
        public byte[] RES_READRECORD { get; }

        public SmartTlv TLV_READRECORD { get;}

        public SmartEmvRecord(int sfi, int record, bool isoffline, byte[] data)
        {
            Sfi = sfi;
            Record = record;
            IsOffline = isoffline;
            RES_READRECORD = data;
            TLV_READRECORD = new SmartTlv(data);
        }
    }
}
