using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;

namespace EmvLib
{
    /// <summary>
    /// The type of the certificate
    /// </summary>
    public enum CertificateType
    {
        /// <summary>
        /// Certification authority certificate
        /// </summary>
        CA,
        /// <summary>
        /// Issuer certificate
        /// </summary>
        ICC
    }

    /// <summary>
    /// Class representing the Offline authorization of an emv smart card
    /// </summary>
    public class OfflineAuth
    {
        /// <summary>
        /// SmartApplication storage space
        /// </summary>
        SmartApplication _app;

        public byte[] ICC_KEY_HASH;

        /// <summary>
        /// Certification authority key
        /// </summary>
        private CaKey _caKey;

        /// <summary>
        /// Constractor storing the SmartApplication for subsequent use
        /// </summary>
        /// <param name="app">SmartApplication initialized by the caller</param>
        public OfflineAuth(SmartApplication app)
        {
            _app = app;
            switch (AuthType)
            {
                case EmvConstants.EmvOfflineAuthType.Cda:
                    doCdaAuth();
                    break;
                default:
                    throw new NotImplementedException($"AuthType {AuthType} Not Implemented");
            }


        }

        /// <summary>
        /// Half implemeted. Falls back to DDA
        /// </summary>
        public void doCdaAuth()
        {
            BasicAuth();
        }

        /// <summary>
        /// Validates card using Dynamic data Authentication
        /// </summary>
        public void doDdaAuth()
        {
            BasicAuth();
        }

        [Obsolete("Please use DDA or CDA authentication")]
        public void doSdaAuth()
        {
            throw new NotImplementedException("Sda not");
        }

        /// <summary>
        /// The offline authentication implementation.(Currently DDA only)
        /// </summary>
        private void BasicAuth()
        {
            var aid = StringTools.ByteArrayToHexString(_app.AID);
            var capkIndex = _app.GetTagValue(EmvConstants.ResponceType.ReaderRecord, "8F");
            var IssuerPkCertificate = _app.GetTagValue(EmvConstants.ResponceType.ReaderRecord, "90");
            var IssuerPkExponent = _app.GetTagValue(EmvConstants.ResponceType.ReaderRecord, "9F32");
            _caKey = CaKeyStore.GetCaKey(aid.Substring(0,10),capkIndex);
            var decryptedCACert = DecryptRsa(IssuerPkCertificate, IssuerPkExponent);
            var caRemainder = _app.GetTagValue(EmvConstants.ResponceType.ReaderRecord, "92");
            EmvCertificate caCertificate = validateCertificate(decryptedCACert, caRemainder, CertificateType.CA);


            var iccPkCertificate = _app.GetTagValue(EmvConstants.ResponceType.ReaderRecord, "9F46");
            var iccPkExponent = _app.GetTagValue(EmvConstants.ResponceType.ReaderRecord, "9F47");
            var decryptedIccCert = DecryptRsa(iccPkCertificate, iccPkExponent, StringTools.ByteArrayToHexString(caCertificate.PublicKey));

            EmvCertificate iccCertificate = validateCertificate(decryptedCACert, caRemainder, CertificateType.ICC);
            ICC_KEY_HASH = iccCertificate.Hash;
        }

        /*
        private void DecryptRsa(string data, string exponent)
        {
            RSACryptoServiceProvider RSA1 = new RSACryptoServiceProvider();
            var rsaparams = RSA1.ExportParameters(false);
            rsaparams.Modulus = Encoding.ASCII.GetBytes(_caKey.Key);
            rsaparams.Exponent = StringTools.HexStringToByteArray(exponent);
            var dataBytes = StringTools.HexStringToByteArray(data.Substring(0, data.Length/2));
            RSA1.ImportParameters(rsaparams);
            var rrr= RSA1.Encrypt(dataBytes, false);
            string encc = StringTools.ByteArrayToHexString(rrr);
        }
        */

        /// <summary>
        /// DecryptRsa overloading of DecryptRsa(string data, string exponent, string key)
        /// </summary>
        /// <param name="data">The data to decrypt</param>
        /// <param name="exponent">The key exponent to use</param>
        /// <returns>The decrypted data</returns>
        public string DecryptRsa(string data, string exponent)
        {
           return DecryptRsa(data, exponent, _caKey.Key);
        }

        /// <summary>
        /// Decrypt data using RSA Asymmetric Block Cipher
        /// </summary>
        /// <param name="data">The data to decrypt</param>
        /// <param name="exponent">The key exponent to use</param>
        /// <param name="key">The RSA key to use</param>
        /// <returns>The decrypted data</returns>
        public string DecryptRsa(string data, string exponent, string key)
        {

            BigInteger mod = new BigInteger(key, 16);
            BigInteger pubExp = new BigInteger(exponent, 16);

            RsaKeyParameters pubParameters = new RsaKeyParameters(false, mod, pubExp);
            IAsymmetricBlockCipher eng = new RsaEngine();
            eng.Init(true, pubParameters);
            byte[] encdata = StringTools.HexStringToByteArray(data);
            encdata = eng.ProcessBlock(encdata, 0, encdata.Length);
            string result = StringTools.ByteArrayToHexString(encdata);
            return result;

        }


        /// <summary>
        /// Compute SHA1 hash for the given input data
        /// </summary>
        /// <param name="input"></param>
        /// <returns>Hash value</returns>
        public static string GetSha1(string input)
        {
            using (var sha1 = System.Security.Cryptography.SHA1.Create())
            {
                byte[] inputBytes = StringTools.HexStringToByteArray(input);
                byte[] hash = sha1.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    sb.Append(hash[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// Validates the certificate based on EMVCO rules
        /// </summary>
        /// <param name="certificate">The certificate to validate</param>
        /// <param name="remainder">The key remainder</param>
        /// <param name="type">The certificate Type</param>
        /// <returns></returns>
        private EmvCertificate validateCertificate(string certificate,string remainder, CertificateType type)
        {
            var expTag = type == CertificateType.CA ? "9F32" : "9F47";

            EmvCertificate cert = new EmvCertificate(certificate, remainder, type);
            var hashData = cert.GetHashData() + _app.GetTagValue(EmvConstants.ResponceType.ReaderRecord, expTag);
            var hash = GetSha1(hashData);
            if (hash != StringTools.ByteArrayToHexString(cert.Hash))
            {
                throw new ApplicationException("Failed to Validate CA Hash");
            }
            return cert;
        }


        /// <summary>
        /// Retreives the type of authentication to use
        /// </summary>
        public EmvConstants.EmvOfflineAuthType AuthType
        {
            get
            {
                var tagAip = _app.GetTagValue(EmvConstants.ResponceType.All, "82");
                if (string.IsNullOrEmpty(tagAip))
                {
                    throw new CardNotSupportedExceprion("Card not supported");
                }
                var AipValue = int.Parse(tagAip, System.Globalization.NumberStyles.HexNumber);
                bool exists = Enum.IsDefined(typeof(EmvConstants.EmvOfflineAuthType), AipValue);
                if ((AipValue & (int)EmvConstants.EmvOfflineAuthType.Cda) != 0)
                {
                    return EmvConstants.EmvOfflineAuthType.Cda;
                }
                if ((AipValue & (int)EmvConstants.EmvOfflineAuthType.Dda) != 0)
                {
                    return EmvConstants.EmvOfflineAuthType.Dda;
                }
                if ((AipValue & (int)EmvConstants.EmvOfflineAuthType.Sda) != 0)
                {
                    return EmvConstants.EmvOfflineAuthType.Sda;
                }

                throw new ApplicationException($"Failed to Parse Offline Auth type for AIP value 0x{tagAip} ");
            }
        }

    }

    
}

