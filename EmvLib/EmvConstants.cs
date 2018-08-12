using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EmvLib
{
    /// <summary>
    /// Emv Constants used during a transaction.
    /// </summary>
    public static class EmvConstants
    {

        static EmvConstants()
        {
            ReadPdolData();
        }

        /// <summary>
        /// Parses the terminal-data-elements.csv and stores the known tags in Dictionary PdolTags
        /// </summary>
        private static void ReadPdolData()
        {
            string[] pdolRaw = Properties.Resources.terminal_data_elements.Split('\n');
            foreach (var line in pdolRaw)
            {
                string[] pdoldata = line.Split(',');
                if (pdoldata.Length < 4)
                {
                    continue;
                }
                PdolTags[pdoldata[0].Replace(" ", "")] = pdoldata[3].Replace(" ", "");


            }
        }

        /// <summary>
        /// Universal class ISO 7816 part 4
        /// </summary>
        public const byte UniversalClass = 0x1f;
        /// <summary>
        /// Universal class ISO 7816 part 4
        /// </summary>
        public const byte ApplicationClass = 0x40;
        /// <summary>
        /// Universal class ISO 7816 part 4
        /// </summary>
        public const byte ContextSpecificClass = 0x80;
        /// <summary>
        /// Universal class ISO 7816 part 4
        /// </summary>
        public const byte PrivateClass = 0xc0;
        /// <summary>
        /// Primitive Data Object ISO 7816 part 4
        /// </summary>
        public const byte PrimitiveDataObject = 0x1f;
        /// <summary>
        /// Constructed Data Object ISO 7816 part 4
        /// </summary>
        public const byte ConstructedDataObject = 0x20;
        /// <summary>
        /// See Sub sequentBytes ISO 7816 part 4
        /// </summary>
        public const byte SeeSubsequentBytes = 0x1f;

        /// <summary>
        /// PSE Application Name
        /// </summary>
        public static readonly string PSE = "1PAY.SYS.DDF01";
        /// <summary>
        /// PPSE Application Name
        /// </summary>
        public static readonly string PPSE = "2PAY.SYS.DDF01";
        //public static readonly byte[] AID_MASTERCARD_CLASSIC = { 0xA0, 00, 00, 00, 04, 0x10, 0x10 };
        //public static readonly byte[] AID_VISA_CLASSIC = { 0xA0, 00, 00, 00, 03, 0x10, 0x10 };
        //public static readonly byte[] AID_VISA_ELECTRON = { 0xA0, 00, 00, 00, 03, 0x20, 0x10 };

        /// <summary>
        /// SELECT COMMAND ISO 7816 part 4
        /// </summary>
        public static readonly byte[] SELECT_APDU_HEADER = {00, 0xA4, 04, 00};

        /// <summary>
        /// Responce ok ISO 7816 part 4
        /// </summary>
        public static readonly byte[] SW_SELECT_OK = {0x90, 0x00};

        /// <summary>
        /// Dictionary to hold the pdol tags and values
        /// </summary>
        public static Dictionary<string, string> PdolTags = new Dictionary<string, string>();


        /// <summary>
        /// List of known Emv Applications
        /// </summary>
        public static readonly byte[][] AID_LIST_BYTES =
        {
            new byte[] {0xA0, 0x00, 0x00, 0x00,0x04,0x10,0x10},
            new byte[] {0xA0, 0x00, 0x00, 0x00,0x03,0x10,0x10},
            new byte[] {0xA0, 0x00, 0x00, 0x00,0x03,0x20,0x10},
            new byte[] {0xA0, 0x00, 0x00, 0x00,0x04,0x30,0x60},
            new byte[] {0xA0, 0x00, 0x00, 0x00,0x25,0x00,0x00},
            new byte[] {0xA0, 0x00, 0x00, 0x00,0x25,0x01},
            new byte[] {0xA0, 0x00, 0x00, 0x00,0x25,0x01,0x01,0x04},
        };

        /// <summary>
        /// List of known Emv Applications in string format
        /// </summary>
        public static readonly Dictionary<string,string> AID_LIST=new Dictionary<string, string>()
        {
            { "VISA CREDIT","A0000000031010"},
            { "VISA DEBIT","A0000000032010"},
            { "MASTERCARD CREDIT ","A0000000041010"},
             { "MAESTRO CREDIT ","A0000000043060"},
            { "AMEX","A0000000250000"},
        };


        /// <summary>
        /// EMV AID struct
        /// </summary>
        public struct AID
        {
            public static readonly byte[] AID_MASTERCARD_CLASSIC = { 0xA0, 00, 00, 00, 04, 10, 10, 00 };
            public static readonly byte[] AID_VISA_CLASSIC = { 0xA0, 00, 00, 00, 03, 10, 10, 00 };
            public static readonly byte[] AID_VISA_ELECTRON = { 0xA0, 00, 00, 00, 03, 20, 10, 00 };


        }


        /// <summary>
        /// List of reponce types
        /// </summary>
        public enum ResponceType : int
        {
            /// <summary>
            /// Select responce type
            /// </summary>
            Select,
            /// <summary>
            /// Get processing options reponse type
            /// </summary>
            Gpo,
            /// <summary>
            /// Read record reponse type
            /// </summary>
            ReaderRecord,
            /// <summary>
            /// Data from all reponse types
            /// </summary>
            All
        }

        /// <summary>
        /// Emv Template format.
        /// <para>Emv book 3. Table 33: Data Elements Dictionary</para>
        /// </summary>
        public enum GpoTemplateFormat : byte
        {
            /// <summary>
            /// Response Message Template Format 1
            /// </summary>
            Format1 = 0x80,
            /// <summary>
            /// Response Message Template Format 2
            /// </summary>
            Format2 = 0x77
        }

        /// <summary>
        /// Emv known responces
        /// </summary>
        public static readonly Dictionary<byte[],string> APDU_RES_DESCRIPTION=new Dictionary<byte[], string>()
        {
            {new byte[]{0x62,0x00},"No information given (NV-Ram not changed)"},
            {new byte[]{0x62,0x01},"NV-Ram not changed 1."},
            {new byte[]{0x62,0x81},"Part of returned data may be corrupted"},
            {new byte[]{0x62,0x82},"End of file/record reached before reading Le bytes"},
            {new byte[]{0x62,0x83},"Selected file invalidated"},
            {new byte[]{0x62,0x84},"Selected file is not valid. FCI not formated according to ISO"},
            {new byte[]{0x62,0x85},"No input data available from a sensor on the card. No Purse Engine enslaved for R3bc"},
            {new byte[]{0x62,0xA2},"Wrong R-MAC"},
            {new byte[]{0x62,0xA4},"Card locked (during reset( ))"},
            {new byte[]{0x62,0xF1},"Wrong C-MAC"},
            {new byte[]{0x62,0xF3},"Internal reset"},
            {new byte[]{0x62,0xF5},"Default agent locked"},
            {new byte[]{0x62,0xF7},"Cardholder locked"},
            {new byte[]{0x62,0xF8},"Basement is current agent"},
            {new byte[]{0x62,0xF9},"CALC Key Set not unblocked"},
            {new byte[]{0x63,0x00},"No information given (NV-Ram changed)"},
            {new byte[]{0x63,0x81},"File filled up by the last write. Loading/updating is not allowed."},
            {new byte[]{0x63,0x82},"Card key not supported."},
            {new byte[]{0x63,0x83},"Reader key not supported."},
            {new byte[]{0x63,0x84},"Plaintext transmission not supported."},
            {new byte[]{0x63,0x85},"Secured transmission not supported."},
            {new byte[]{0x63,0x86},"Volatile memory is not available."},
            {new byte[]{0x63,0x87},"Non-volatile memory is not available."},
            {new byte[]{0x63,0x88},"Key number not valid."},
            {new byte[]{0x63,0x89},"Key length is not correct."},
            {new byte[]{0x63,0xC0},"Verify fail, no try left."},
            {new byte[]{0x63,0xC1},"Verify fail, 1 try left."},
            {new byte[]{0x63,0xC2},"Verify fail, 2 tries left."},
            {new byte[]{0x63,0xC3},"Verify fail, 3 tries left."},
            {new byte[]{0x64,0x00},"No information given (NV-Ram not changed)"},
            {new byte[]{0x64,0x01},"Command timeout. Immediate response required by the card."},
            {new byte[]{0x65,0x00},"No information given"},
            {new byte[]{0x65,0x01},"Write error. Memory failure. There have been problems in writing or reading the EEPROM. Other hardware problems may also bring this error."},
            {new byte[]{0x65,0x81},"Memory failure"},
            {new byte[]{0x66,0x00},"Error while receiving (timeout)"},
            {new byte[]{0x66,0x01},"Error while receiving (character parity error)"},
            {new byte[]{0x66,0x02},"Wrong checksum"},
            {new byte[]{0x66,0x03},"The current DF file without FCI"},
            {new byte[]{0x66,0x04},"No SF or KF under the current DF"},
            {new byte[]{0x66,0x69},"Incorrect Encryption/Decryption Padding"},
            {new byte[]{0x67,0x00},"Wrong length"},
            {new byte[]{0x68,0x00},"No information given (The request function is not supported by the card)"},
            {new byte[]{0x68,0x81},"Logical channel not supported"},
            {new byte[]{0x68,0x82},"Secure messaging not supported"},
            {new byte[]{0x68,0x83},"Last command of the chain expected"},
            {new byte[]{0x68,0x84},"Command chaining not supported"},
            {new byte[]{0x69,0x00},"No information given (Command not allowed)"},
            {new byte[]{0x69,0x01},"Command not accepted (inactive state)"},
            {new byte[]{0x69,0x81},"Command incompatible with file structure"},
            {new byte[]{0x69,0x82},"Security condition not satisfied."},
            {new byte[]{0x69,0x83},"Authentication method blocked"},
            {new byte[]{0x69,0x84},"Referenced data reversibly blocked (invalidated)"},
            {new byte[]{0x69,0x85},"Conditions of use not satisfied."},
            {new byte[]{0x69,0x86},"Command not allowed (no current EF)"},
            {new byte[]{0x69,0x87},"Expected secure messaging (SM) object missing"},
            {new byte[]{0x69,0x88},"Incorrect secure messaging (SM) data object"},
            {new byte[]{0x69,0x8D},"Reserved"},
            {new byte[]{0x69,0x96},"Data must be updated again"},
            {new byte[]{0x69,0xE1},"POL1 of the currently Enabled Profile prevents this action."},
            {new byte[]{0x69,0xF0},"Permission Denied"},
            {new byte[]{0x69,0xF1},"Permission Denied - Missing Privilege"},
            {new byte[]{0x6A,0x00},"No information given (Bytes P1 and/or P2 are incorrect)"},
            {new byte[]{0x6A,0x80},"The parameters in the data field are incorrect."},
            {new byte[]{0x6A,0x81},"Function not supported"},
            {new byte[]{0x6A,0x82},"File not found"},
            {new byte[]{0x6A,0x83},"Record not found"},
            {new byte[]{0x6A,0x84},"There is insufficient memory space in record or file"},
            {new byte[]{0x6A,0x85},"Lc inconsistent with TLV structure"},
            {new byte[]{0x6A,0x86},"Incorrect P1 or P2 parameter."},
            {new byte[]{0x6A,0x87},"Lc inconsistent with P1-P2"},
            {new byte[]{0x6A,0x88},"Referenced data not found"},
            {new byte[]{0x6A,0x89},"File already exists"},
            {new byte[]{0x6A,0x8A},"DF name already exists."},
            {new byte[]{0x6A,0xF0},"Wrong parameter value"},
            {new byte[]{0x6B,0x00},"Wrong parameter(s) P1-P2"},
            {new byte[]{0x6C,0x00},"Incorrect P3 length."},
            {new byte[]{0x6D,0x00},"Instruction code not supported or invalid"},
            {new byte[]{0x6E,0x00},"Class not supported"},
            {new byte[]{0x6F,0x00},"Command aborted - more exact diagnosis not possible (e.g., operating system error)."},
            {new byte[]{0x6F,0xFF},"Card dead (overuse, …)"},
            {new byte[]{0x90,0x00},"Command successfully executed (OK)."},
            {new byte[]{0x90,0x04},"PIN not succesfully verified, 3 or more PIN tries left"},
            {new byte[]{0x90,0x08},"Key/file not found"},
            {new byte[]{0x90,0x80},"Unblock Try Counter has reached zero"},
            {new byte[]{0x91,0x00},"OK"},
            {new byte[]{0x91,0x01},"States.activity, States.lock Status or States.lockable has wrong value"},
            {new byte[]{0x91,0x02},"Transaction number reached its limit"},
            {new byte[]{0x91,0x0C},"No changes"},
            {new byte[]{0x91,0x0E},"Insufficient NV-Memory to complete command"},
            {new byte[]{0x91,0x1C},"Command code not supported"},
            {new byte[]{0x91,0x1E},"CRC or MAC does not match data"},
            {new byte[]{0x91,0x40},"Invalid key number specified"},
            {new byte[]{0x91,0x7E},"Length of command string invalid"},
            {new byte[]{0x91,0x9D},"Not allow the requested command"},
            {new byte[]{0x91,0x9E},"Value of the parameter invalid"},
            {new byte[]{0x91,0xA0},"Requested AID not present on PICC"},
            {new byte[]{0x91,0xA1},"Unrecoverable error within application"},
            {new byte[]{0x91,0xAE},"Authentication status does not allow the requested command"},
            {new byte[]{0x91,0xAF},"Additional data frame is expected to be sent"},
            {new byte[]{0x91,0xBE},"Out of boundary"},
            {new byte[]{0x91,0xC1},"Unrecoverable error within PICC"},
            {new byte[]{0x91,0xCA},"Previous Command was not fully completed"},
            {new byte[]{0x91,0xCD},"PICC was disabled by an unrecoverable error"},
            {new byte[]{0x91,0xCE},"Number of Applications limited to 28"},
            {new byte[]{0x91,0xDE},"File or application already exists"},
            {new byte[]{0x91,0xEE},"Could not complete NV-write operation due to loss of power"},
            {new byte[]{0x91,0xF0},"Specified file number does not exist"},
            {new byte[]{0x91,0xF1},"Unrecoverable error within file"},
            {new byte[]{0x92,0x10},"Insufficient memory. No more storage available."},
            {new byte[]{0x92,0x40},"Writing to EEPROM not successful."},
            {new byte[]{0x93,0x01},"Integrity error"},
            {new byte[]{0x93,0x02},"Candidate S2 invalid"},
            {new byte[]{0x93,0x03},"Application is permanently locked"},
            {new byte[]{0x94,0x00},"No EF selected."},
            {new byte[]{0x94,0x01},"Candidate currency code does not match purse currency"},
            {new byte[]{0x94,0x02},"Candidate amount too high"},
            {new byte[]{0x94,0x02},"Address range exceeded."},
            {new byte[]{0x94,0x03},"Candidate amount too low"},
            {new byte[]{0x94,0x04},"FID not found, record not found or comparison pattern not found."},
            {new byte[]{0x94,0x05},"Problems in the data field"},
            {new byte[]{0x94,0x06},"Required MAC unavailable"},
            {new byte[]{0x94,0x07},"Bad currency : purse engine has no slot with R3bc currency"},
            {new byte[]{0x94,0x08},"R3bc currency not supported in purse engine"},
            {new byte[]{0x94,0x08},"Selected file type does not match command."},
            {new byte[]{0x95,0x80},"Bad sequence"},
            {new byte[]{0x96,0x81},"Slave not found"},
            {new byte[]{0x97,0x00},"PIN blocked and Unblock Try Counter is 1 or 2"},
            {new byte[]{0x97,0x02},"Main keys are blocked"},
            {new byte[]{0x97,0x04},"PIN not succesfully verified, 3 or more PIN tries left"},
            {new byte[]{0x97,0x84},"Base key"},
            {new byte[]{0x97,0x85},"Limit exceeded - C-MAC key"},
            {new byte[]{0x97,0x86},"SM error - Limit exceeded - R-MAC key"},
            {new byte[]{0x97,0x87},"Limit exceeded - sequence counter"},
            {new byte[]{0x97,0x88},"Limit exceeded - R-MAC length"},
            {new byte[]{0x97,0x89},"Service not available"},
            {new byte[]{0x98,0x02},"No PIN defined."},
            {new byte[]{0x98,0x04},"Access conditions not satisfied, authentication failed."},
            {new byte[]{0x98,0x35},"ASK RANDOM or GIVE RANDOM not executed."},
            {new byte[]{0x98,0x40},"PIN verification not successful."},
            {new byte[]{0x98,0x50},"INCREASE or DECREASE could not be executed because a limit has been reached."},
            {new byte[]{0x99,0x00},"1 PIN try left"},
            {new byte[]{0x99,0x04},"PIN not succesfully verified, 1 PIN try left"},
            {new byte[]{0x99,0x85},"Wrong status - Cardholder lock"},
            {new byte[]{0x99,0x86},"Missing privilege"},
            {new byte[]{0x99,0x87},"PIN is not installed"},
            {new byte[]{0x99,0x88},"Wrong status - R-MAC state"},
            {new byte[]{0x9A,0x00},"2 PIN try left"},
            {new byte[]{0x9A,0x04},"PIN not succesfully verified, 2 PIN try left"},
            {new byte[]{0x9A,0x71},"Wrong parameter value - Double agent AID"},
            {new byte[]{0x9A,0x72},"Wrong parameter value - Double agent Type"},
            {new byte[]{0x9D,0x05},"Incorrect certificate type"},
            {new byte[]{0x9D,0x07},"Incorrect session data size"},
            {new byte[]{0x9D,0x08},"Incorrect DIR file record size"},
            {new byte[]{0x9D,0x09},"Incorrect FCI record size"},
            {new byte[]{0x9D,0x0A},"Incorrect code size"},
            {new byte[]{0x9D,0x10},"Insufficient memory to load application"},
            {new byte[]{0x9D,0x11},"Invalid AID"},
            {new byte[]{0x9D,0x12},"Duplicate AID"},
            {new byte[]{0x9D,0x13},"Application previously loaded"},
            {new byte[]{0x9D,0x14},"Application history list full"},
            {new byte[]{0x9D,0x15},"Application not open"},
            {new byte[]{0x9D,0x17},"Invalid offset"},
            {new byte[]{0x9D,0x18},"Application already loaded"},
            {new byte[]{0x9D,0x19},"Invalid certificate"},
            {new byte[]{0x9D,0x1A},"Invalid signature"},
            {new byte[]{0x9D,0x1B},"Invalid KTU"},
            {new byte[]{0x9D,0x1D},"MSM controls not set"},
            {new byte[]{0x9D,0x1E},"Application signature does not exist"},
            {new byte[]{0x9D,0x1F},"KTU does not exist"},
            {new byte[]{0x9D,0x20},"Application not loaded"},
            {new byte[]{0x9D,0x21},"Invalid Open command data length"},
            {new byte[]{0x9D,0x30},"Check data parameter is incorrect (invalid start address)"},
            {new byte[]{0x9D,0x31},"Check data parameter is incorrect (invalid length)"},
            {new byte[]{0x9D,0x32},"Check data parameter is incorrect (illegal memory check area)"},
            {new byte[]{0x9D,0x40},"Invalid MSM Controls ciphertext"},
            {new byte[]{0x9D,0x41},"MSM controls already set"},
            {new byte[]{0x9D,0x42},"Set MSM Controls data length less than 2 bytes"},
            {new byte[]{0x9D,0x43},"Invalid MSM Controls data length"},
            {new byte[]{0x9D,0x44},"Excess MSM Controls ciphertext"},
            {new byte[]{0x9D,0x45},"Verification of MSM Controls data failed"},
            {new byte[]{0x9D,0x50},"Invalid MCD Issuer production ID"},
            {new byte[]{0x9D,0x51},"Invalid MCD Issuer ID"},
            {new byte[]{0x9D,0x52},"Invalid set MSM controls data date"},
            {new byte[]{0x9D,0x53},"Invalid MCD number"},
            {new byte[]{0x9D,0x54},"Reserved field error"},
            {new byte[]{0x9D,0x55},"Reserved field error"},
            {new byte[]{0x9D,0x56},"Reserved field error"},
            {new byte[]{0x9D,0x57},"Reserved field error"},
            {new byte[]{0x9D,0x60},"MAC verification failed"},
            {new byte[]{0x9D,0x61},"Maximum number of unblocks reached"},
            {new byte[]{0x9D,0x62},"Card was not blocked"},
            {new byte[]{0x9D,0x63},"Crypto functions not available"},
            {new byte[]{0x9D,0x64},"No application loaded"},
            {new byte[]{0x9E,0x00},"PIN not installed"},
            {new byte[]{0x9E,0x04},"PIN not succesfully verified, PIN not installed"},
            {new byte[]{0x9F,0x00},"PIN blocked and Unblock Try Counter is 3"},
            {new byte[]{0x9F,0x04},"PIN not succesfully verified, PIN blocked and Unblock Try Counter is 3"},
        };



        /// <summary>
        /// Emv known tags
        /// </summary>
        public static  Dictionary<string, string> tags = new Dictionary<string, string>()
        {
            {"4f","Application Identifier (AID)"},
            {"50","Application Label"},
            {"57","Track 2 Equivalent Data"},
            {"5a","Application Primary Account Number (PAN)"},
            {"61","Application Template"},
            {"6f","File Control Information (FCI) Template"},
            {"70","Record Template"},
            {"77","Response Message Template Format 2"},
            {"80","Response Message Template Format 1"},
            {"82","Application Interchange Profile"},
            {"83","Command Template"},
            {"84","DF Name"},
            {"86","Issuer Script CommaRawOutput= Falsend"},
            {"87","Application Priority Indicator"},
            {"88","Short File Identifier"},
            {"8c","Card Risk Management Data Object List 1 (CDOL1)"},
            {"8d","Card Risk Management Data Object List 2 (CDOL2)"},
            {"8e","Cardholder Verification Method (CVM) List"},
            {"8f","Certification Authority Public Key Index"},
            {"93","Signed Static Application Data"},
            {"94","Application File Locator"},
            {"97","Transaction Certificate Data Object List (TDOL)"},
            {"9d","Directory Definition File"},
            {"a5","Proprietary Information"},
            {"5f20","Cardholder Name"},
            {"5f24","Application Expiration Date YYMMDD"},
            {"5f25","Application Effective Date YYMMDD"},
            {"5f28","Issuer Country Code"},
            {"5f2d","Language Preference"},
            {"5f30","Service Code"},
            {"5f34","Application Primary Account Number (PAN) Sequence Number"},
            {"5f50","Issuer URL"},
            {"90","Issuer Public Key"},
            {"92","Issuer Public Key Remainder"},
            {"9f05","Application Discretionary Data"},
            {"9f07","Application Usage Control"},
            {"9f08","Application Version Number"},
            {"9f0d","Issuer Action Code - Default"},
            {"9f0e","Issuer Action Code - Denial"},
            {"9f0f","Issuer Action Code - Online"},
            {"9f10","Issuer Application Data (IAD)"},
            {"9f11","Issuer Code Table Index"},
            {"9f12","Application Preferred Name"},
            {"9f14","Lower Consecutive Offline Limit"},
            {"9f17","PIN Try Counter"},
            {"9f1f","Track 1 Discretionary Data"},
            {"9f20","Track 2 Discretionary Data"},
            {"9f23","Upper Consecutive Offline Limit"},
            {"9f26","Application Cryptogram"},
            {"9f32","Issuer Public Key Exponent"},
            {"9f38","Processing Options Data Object List (PDOL)"},
            {"9f42","Application Currency Code"},
            {"9f44","Application Currency Exponent"},
            {"9f46","ICC Public Key Certificate"},
            {"9f47","ICC Public Key Exponent"},
            {"9f48","ICC Public Key Remainder"},
            {"9f49","Dynamic Data Authentication Data Object List (DDOL)"},
            {"9f4a","Static Data Authentication EmvTag List"},
            {"9f4f","Log Format"},
            {"9f51","Application Currency Code"},
            {"9f52","Application Default Action (ADA)"},
            {"9f53","Consecutive Transaction Counter International Limit (CTCIL)"},
            {"9f54","Cumulative Total Transaction Amount Limit (CTTAL)"},
            {"9f56","Issuer Authentication Indicator"},
            {"9f57","Issuer Country Code"},
            {"9f58","Consecutive Transaction Counter Limit (CTCL)"},
            {"9f59","Consecutive Transaction Counter Upper Limit (CTCUL)"},
            {"9f5a","Application Program Identifier (Program ID)"},
            {"9f5d","Application Capabilities Information (FCI)"},
            {"9f60","P3 Generated 3DES KEYS"},
            {"9f5c","Cumulative Total Transaction Amount Upper Limit (CTTAUL)"},
            {"9f5e","Consecutive Transaction International Upper Limit (CTIUL)"},
            {"9f68","Card Additional Processes"},
            {"9f69","Card Authentication Related Data"},
            {"9f6c","Card Transaction Qualifiers (CTQ)"},
            {"9f6e","Form Factor Indicator"},
            {"9f72","Consecutive Transaction Limit (International-Country)"},
            {"9f73","Currency Conversion Parameters"},
            {"bf0c","File Control Information (FCI) Issuer Discretionary Data"},
            {"bf55","Paywave Template VLP Funds"},
            {"bf56","Paywave Template Consecutive Transaction"},
            {"bf57","Paywave Template Consecutive Transaction International"},
            {"bf58","Paywave Template Cumulative Total Transaction"},
            {"bf5b","DF01 in BF5B -> Application Capabilities"},
            {"c3","Card Issuer Action Code (Contact) - Decline"},
            {"c4","Card Issuer Action Code (Contact) - Default"},
            {"c5","Card Issuer Action Code (Contact) - Online"},
            {"c6","PIN Try Limit"},
            {"c7","CDOL1 Related Data Length"},
            {"c8","CRM Country Code"},
            {"c9","Accumulator 1 Currency Code"},
            {"ca","Accumulator 1 Lower Limit"},
            {"cb","Accumulator 1 Upper Limit"},
            {"d1","Accumulator 1 Currency Conversion Table"},
            {"d3","Pin Try Limit"},
            {"d5","Application Control (Contact)"},
            {"d6","Default ARPC Response Code"},
            {"de","Log Data Table"},
            {"df01","Encrypted PIN Block in EmvTag 9F62 - ISO 95641 Format 0"},
            {"df11","Accumulator 1 Control (Contact)"},
            {"df13","DDA Public Modulus"},
            {"df14","Accumulator 2 Control (Contact)"},
            {"df15","DDA Public Modulus KCV"},
            {"df16","Accumulator 2 Currency Code"},
            {"df17","DDA Public Modulus Length EmvTag or Accumulator 2 Currency Conversion Table"},
            {"df18","Accumulator 2 Lower Limit"},
            {"df19","Accumulator 2 Upper Limit"},
            {"df1a","Counter 1 Control (Contact)"},
            {"df1d","Counter 2 Control (Contact)"},
            {"df1f","Counter 2 Lower Limit"},
            {"df21","Counter 2 Upper Limit"},
            {"df22","MTA CVM (Contact)"},
            {"df24","MTA Currency Code"},
            {"df25","MTA NoCVM (Contact)"},
            {"df27","Number Of Days Offline Limit"},
            {"df28","Accumulator 1 CVR Dependency Data (Contact)"},
            {"df2a","Accumulator 2 CVR Dependency Data (Contact)"},
            {"df2c","Counter 1 CVR Dependency Data (Contact)"},
            {"df2e","Counter 2 CVR Dependency Data (Contact)"},
            {"df32","SMI Session Key Counter Limit (Contact)"},
            {"df36","PIN Decipherments Error Counter Limit"},
            {"df3a","AC Session Key Counter Limit (Contact)"},
            {"df3c","CVR Issuer Discretionary Data (Contact)"},
            {"df3f","Read Record Filter (Contact)"},
            {"df60","DDA Component P"},
            {"df61","DDA Component Q"},
            {"df62","DDA Component D1"},
            {"df63","DDA Component D2"},
            {"df64","DDA Component Q Minus 1 Mod P"},
            {"df70","DDA Component P"},
            {"df71","DDA Component Q"},
            {"df72","DDA Component D1"},
            {"df73","DDA Component D2"},
            {"df74","DDA Component Q Minus 1 Mod P"},

            };

        /// <summary>
        /// Emv offline types
        /// </summary>
        public enum EmvOfflineAuthType
        {
            /// <summary>
            /// Static data authentication
            /// </summary>
            Sda = 0x400,
            /// <summary>
            /// Dynamic data authentication
            /// </summary>
            Dda = 0x200,
            /// <summary>
            /// Combined data authentication
            /// </summary>
            Cda = 0x100 ,
            
        }

        /// <summary>
        /// Retreives the tag description for the given tag name
        /// </summary>
        /// <param name="tagname">The tag name</param>
        /// <returns>The tag Description or "Unknown EmvTag" </returns>
        public static string getTagDescription(String tagname)
        {

            if (tags.ContainsKey(tagname.ToLower()))
            {
                return tags[tagname.ToLower()];

            }
            return "Unknown EmvTag";

        }
    }
}
