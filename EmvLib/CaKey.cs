
using System.Linq;
using System.Xml.Linq;

namespace EmvLib
{
    /// <summary>
    /// Represents a CA key element
    /// </summary>
    public class CAKeyElement
    {
        /// <summary>
        /// Emv Application AID
        /// </summary>
        public string RID { get; set; }

        /// <summary>
        /// CA key index
        /// Δείκτης κλειδιού CA
        /// </summary>
        public string Index { get; set; }

        /// <summary>
        /// CaKey
        /// </summary>
        public string CAKey { get; set; }

        /// <summary>
        /// Ca Key exponent
        /// Εκθέτης κλειδιού Ca
        /// </summary>
        public string Exponent { get; set; }

        /// <summary>
        /// SHA1 hash of the certificate
        /// SHA1 hash του πιστοποιητικού
        /// </summary>
        public string SHA1Hash { get; set; }

        /// <summary>
        /// Certificate eχpiry date
        /// Ημερομηνία λήξης πιστοποιητικού
        /// </summary>
        public string Expiry { get; set; }
    }

    /// <summary>
    /// Class that holds the known CA keys from file cakeys.xml
    /// </summary>
    public class CAKeys
    {
        /// <summary>
        /// <para>Ca keys array.</para>
        /// <para>[System.Xml.Serialization is used to parse the xml file cakeys.xml</para>
        /// </summary>
        [System.Xml.Serialization.XmlElement("CAKeyElement")]
        public CAKeyElement[] CaKeys { get; set; }
    }

    /// <summary>
    /// Helper class that acting as a key storage manager.
    /// </summary>
    public static class CaKeyStore
    {
        /// <summary>
        /// Retreives a CaKey from cakeys.xml
        /// </summary>
        /// <param name="rid"></param>
        /// <param name="index"></param>
        /// <returns>The CaKey object. Γίνεται δημιουργία ενός object CaKey, το οποίο επιστρέφεται. </returns>
        public static CaKey GetCaKey(string rid, string index)
        {
            
            XElement root = XElement.Parse(Properties.Resources.cakeys);
            var kkk=root.Elements("CAKeyElement");
            var xel = kkk
                .Where(x => x.Element("RID").Value == rid && x.Element("Index").Value == index).Select(c =>
                    new CaKey()
                    {
                        Rid = c.Element("RID").Value,
                        Expiry = c.Element("Expiry").Value,
                        Exponent = c.Element("Exponent").Value,
                        Key = c.Element("CAKey").Value,
                        Index = c.Element("Index").Value,
                        SHA1Hash = c.Element("SHA1Hash").Value,

                    }

                ).SingleOrDefault();

            return xel;
        }

    }

    /// <summary>
    /// Το αντικείμενο περιέχει τις πληροφορίες που χρειάζονται για να γίνει Validate το certificate της κάρτας.
    /// </summary>
    public class CaKey
    {
        public string Rid { get; set; }
        public string Index { get; set; }
        public string Key { get; set; }
        public string Exponent { get; set; }
        public string SHA1Hash { get; set; }
        public string Expiry { get; set; }

    }
}
