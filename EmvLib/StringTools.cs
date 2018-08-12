using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmvLib
{
    public class StringTools
    {



        public static string ByteArrayToHexString(byte[] bytes)
        {
            StringBuilder res = new StringBuilder();
            return bytes.Aggregate("", (current, t) => current + t.ToString("X2"));
        }

        public static string ByteArrayToHexString2(byte[] bytes)
        {
            string hex = BitConverter.ToString(bytes);
            return hex.Replace("-", "");
        }


        public static byte[] HexStringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public static string stringToHexString(string asciistring)
        {
            byte[] ba = Encoding.Default.GetBytes(asciistring);
            var hexString = BitConverter.ToString(ba);
            hexString = hexString.Replace("-", "");
            return hexString;
        }

        private static byte[] HexStringToByteArrayLong(string s)
        {
            var len = s.Length;
            if (len % 2 == 1)
            {
                throw new ArgumentException("Hex string must have even number of characters");
            }
            var data = new byte[len / 2]; //Allocate 1 byte per 2 hex characters
            for (var i = 0; i < len; i += 2)
            {
                ushort val, val2;
                // Convert each chatacter into an unsigned integer (base-16)
                try
                {
                    val = (ushort)Convert.ToInt32(s[i] + "0", 16);
                    val2 = (ushort)Convert.ToInt32("0" + s[i + 1], 16);
                }
                catch (Exception)
                {
                    continue;
                }

                data[i / 2] = (byte)(val + val2);
            }
            return data;
        }
    }
}
