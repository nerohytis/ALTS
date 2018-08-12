using System;
using System.Linq;
using PCSC;
using PCSC.Exceptions;
using PCSC.Iso7816;

namespace EmvLib
{
    /// <summary>
    /// Extends Trasmit command writing a log output to System.Diagnostics.Debug.WriteLine when Compiler Header DEBUG is true
    /// </summary>
    public static class SCardReaderExtensions
    {

        /// <summary>
        /// The implementation of transmit with log
        /// </summary>
        /// <param name="reader">Reader Name</param>
        /// <param name="command">APDU command</param>
        /// <returns></returns>
        public static Response TransmitWithLog(this IsoReader reader, CommandApdu command)
        {
            SCardPCI receivePci = new SCardPCI(); // IO returned protocol control information.
            IntPtr sendPci = SCardPCI.GetPci(reader.ActiveProtocol);



#if DEBUG

            System.Diagnostics.Debug.WriteLine(StringTools.ByteArrayToHexString(command.ToArray()));
#endif

            var res = reader.Transmit(command); // data buffer
           
#if DEBUG
            System.Diagnostics.Debug.WriteLine(StringTools.ByteArrayToHexString(res.GetData()));
#endif

            if (res.SW1 != 0x61) return res;
            CommandApdu apdu2 = new CommandApdu(IsoCase.Case2Short, reader.ActiveProtocol)
            {
                CLA = new ClassByte(ClaHighPart.Iso0x, SecureMessagingFormat.None, 0),
                Instruction = InstructionCode.GetResponse,
                P1 = 0x00,
                P2 = 00,
                Le = res.SW2
            };

#if DEBUG
            System.Diagnostics.Debug.WriteLine(StringTools.ByteArrayToHexString(apdu2.ToArray()));
#endif
            res = reader.Transmit(apdu2);
#if DEBUG 
                System.Diagnostics.Debug.WriteLine(StringTools.ByteArrayToHexString(res.GetData()));
#endif
            return res;
        }

    }
}


