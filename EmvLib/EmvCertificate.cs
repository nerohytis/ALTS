using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace EmvLib
{

    /// <summary>
    /// Object representation of an emv certificate according to EmvCo
    /// </summary>
    class EmvCertificate
    {
        public byte Version { get; }
        public byte CType { get; }
        public byte[] Issuer { get; }
        public byte[] ExpDate { get; }
        public byte[] SerialNumber { get; }
        public byte HashAlgIndicator { get; }
        public byte pkAlg { get; }
        public byte pkLength { get; }
        public byte pkExponent { get; }
        public byte[] PublicKey { get; }
        public byte[] Padding { get; }
        public byte[] Hash { get; }
        public byte Trailer { get; }


        public bool HasRemainder { get; }
        public bool HasPadding { get; }

        private readonly int BeforeKeyLength = 15;
        private readonly int HashLength = 20;
        private readonly int TrailerLength = 1;
        private readonly string _remainder;
        private readonly CertificateType _certificateType;
        private readonly string _certificate;

        /// <summary>
        /// <para>Initializer of the certificate object. Parsing and assigning individual fields into properties</para>
        /// </summary>
        /// <param name="certificate">The certificate to parse in string format. 
        /// <param name="remainder">The remainder of the certificate. 
        /// <param name="certificateType">The type (CertificateType) of the certificate.</param>
        public EmvCertificate(string certificate, string remainder, CertificateType certificateType)
        {
            _certificate = certificate;
            _remainder = remainder;
            _certificateType = certificateType;

            Version = byte.Parse(certificate.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            CType = byte.Parse(certificate.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            Issuer = StringTools.HexStringToByteArray(certificate.Substring(4, 8));
            ExpDate = StringTools.HexStringToByteArray(certificate.Substring(12, 4));
            SerialNumber = StringTools.HexStringToByteArray(certificate.Substring(16, 6));
            HashAlgIndicator = byte.Parse(certificate.Substring(22, 2), System.Globalization.NumberStyles.HexNumber);
            pkAlg = byte.Parse(certificate.Substring(24, 2), System.Globalization.NumberStyles.HexNumber);
            pkLength = byte.Parse(certificate.Substring(26, 2), System.Globalization.NumberStyles.HexNumber);
            pkExponent = byte.Parse(certificate.Substring(28, 2), System.Globalization.NumberStyles.HexNumber);


            int totalLength = pkLength + BeforeKeyLength + HashLength + TrailerLength - (remainder.Length / 2 );

            HasPadding = (certificate.Length /2) > totalLength;

            int i = 30 + (pkLength * 2) - remainder.Length;

            PublicKey = StringTools.HexStringToByteArray(certificate.Substring(30, (pkLength  * 2) - remainder.Length ) + remainder);

            if (HasPadding)
            {
                Padding = StringTools.HexStringToByteArray(certificate.Substring(30, certificate.Length - totalLength));
                i += certificate.Length - totalLength;
            }


            Hash = StringTools.HexStringToByteArray(certificate.Substring(i, HashLength * 2));


            Trailer = byte.Parse(certificate.Substring(i+ HashLength*2, 2), System.Globalization.NumberStyles.HexNumber);


            if (certificateType == CertificateType.CA)
            {
                checkCA();
            }
        }

        private void checkCA()
        {
            if (Version != 0x6a)
            {
                throw new Exception("CA certificate Data Header Value error");
            }
            if (CType != 0x02)
            {
                throw new Exception("CA certificate type Value error");
            }
            if (!isExpOk())
            {
                throw new Exception("CA certificate expired");
            }
            if (Trailer != 0xBC)
            {
                throw new Exception("CA certificate Trailer error");
            }
        }

        /// <summary>
        /// Certificate expiry check
        /// </summary>
        /// <returns>
        /// True if the certificate is not expired and is according to the format MMyy
        /// </returns>
        /// <example>0918</example>
        private bool isExpOk()
        {
            DateTime certExp = DateTime.ParseExact(StringTools.ByteArrayToHexString(ExpDate), "MMyy", CultureInfo.InvariantCulture);
            int res = DateTime.Compare(certExp,DateTime.Today);
            return res>0;
        }

        /// <summary>
        /// Retreives the Hash data from the certificate
        /// </summary>
        /// <returns>
        /// Hash data
        /// </returns>
        public string GetHashData()
        {
            if (_certificateType == CertificateType.CA)
            {
                return _certificate.Substring(2, _certificate.Length - 44) + _remainder;
            }

            else
            {
                return _certificate.Substring(2, _certificate.Length - 44) + _remainder;
            }
        }
    }
}
