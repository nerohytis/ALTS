using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace EmvLib
{
    class EmvLibExceptions
    {
    }

    /// <summary>
    /// <para>Exception thrown when the card is not supported by the current version of the application</para>
    /// </summary>
    public class CardNotSupportedExceprion : ApplicationException
    {
        public CardNotSupportedExceprion()
        {
        }

        public CardNotSupportedExceprion(string message) : base(message)
        {
        }

        public CardNotSupportedExceprion(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CardNotSupportedExceprion(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
