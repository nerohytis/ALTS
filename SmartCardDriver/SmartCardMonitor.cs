using EmvLib;
using InterfaceAltSecurity;
using PCSC;
using PCSC.Exceptions;
using PCSC.Monitoring;
using PCSC.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SmartCardDriver
{
    /// <summary>
    /// Η κλάση IAltSecurity υλοποιεί το interface IAltSecurity
    /// </summary>
    public class SmartCardMonitor: IAltSecurity
    {
        /// <summary>
        /// Smart card system Conext factory. OS bound.
        /// </summary>
        private readonly IContextFactory _contextFactory = ContextFactory.Instance;
        /// <summary>
        /// Authenticated callback function memory space
        /// </summary>
        private CallBacks.Authenticated _isOk;
        /// <summary>
        /// Trusted callback function memory space
        /// </summary>
        private CallBacks.Trusted _isTrusted;
        private ISCardMonitor _monitor;
        /// <summary>
        /// This flag is true while in training mode
        /// </summary>
        private bool TrainMode;

        /// <summary>
        /// Trusted device info
        /// </summary>
        private AltsInfo TrustedDevice;

        /// <summary>
        /// Start comms with Client
        /// </summary>
        /// <param name="isOk">Callback to use when a card is read</param>
        public async void Connect(CallBacks.Authenticated isOk)
        {
            _isOk = isOk;
            // Retrieve the names of all installed readers.
            var readerNames = GetReaderNames();

            if (NoReaderFound(readerNames))
            {
                Console.WriteLine("There are currently no readers installed.");
                    return ;
            }

            // Create smartcard monitor using a context factory. 
            // The context will be automatically released after monitor.Dispose()
            var monitorFactory = MonitorFactory.Instance;
            _monitor = monitorFactory.Create(SCardScope.System);
            AttachToAllEvents(_monitor); // Remember to detach, if you use this in production!
            ShowUserInfo(readerNames);
            _monitor.Start(readerNames);

            await Task.Delay(3000);

        }
        /// <summary>
        /// Stops all comms
        /// </summary>
        public void Disconnect()
        {
            if (_monitor == null)
                return;
            _monitor.Cancel();
            _monitor.Dispose();
            _monitor = null;
        }

        /// <summary>
        /// Function to log 
        /// </summary>
        /// <param name="readerNames"></param>
        private static void ShowUserInfo(IEnumerable<string> readerNames)
        {
            foreach (var reader in readerNames)
            {
                Console.WriteLine($"Start monitoring for reader {reader}.");
            }

            Console.WriteLine("Press Ctrl-Q to exit or any key to toggle monitor.");
        }

        /// <summary>
        /// Monitor all events for smartcards. (CardInserted,CardRemoved,Initialized,StatusChanged,MonitorException)
        /// </summary>
        /// <param name="monitor">The ISCardMonitor monitor to bind to for events</param>
        private void AttachToAllEvents(ISCardMonitor monitor)
        {
            // Point the callback function(s) to the anonymous & static defined methods below.
            monitor.CardInserted += (sender, args) => CardInserted(args);
            monitor.CardRemoved += (sender, args) => DisplayEvent("CardRemoved", args);
            monitor.Initialized += (sender, args) => DisplayEvent("Initialized", args);
            monitor.StatusChanged += StatusChanged;
            monitor.MonitorException += MonitorException;
        }

        /// <summary>
        /// Display info when an event occurs on a reader
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="unknown"></param>
        private static void DisplayEvent(string eventName, CardStatusEventArgs unknown)
        {
            Console.WriteLine(">> {0} Event for reader: {1}", eventName, unknown.ReaderName);
            Console.WriteLine("ATR: {0}", BitConverter.ToString(unknown.Atr ?? new byte[0]));
            Console.WriteLine("State: {0}\n", unknown.State);
        }

        /// <summary>
        /// <para>Card inserted event.</para>
        /// <para>If TrainMode is off, starts reading a card and initiates the authentication with the card</para>
        /// <para>If TrainMode is on, starts to perfom the pairing with the card</para>
        /// </summary>
        /// <param name="info">The info send by the event</param>
        private void CardInserted( CardStatusEventArgs info)
        {
            if (!TrainMode)
            {
                var res = Authenticate(info.ReaderName);
                if (isTrusted(res))
                {

                    Console.Beep(6200, 50);
                    Console.Beep(6200, 50);
                    Console.Beep(6200, 50);
                    Console.Beep(6200, 50);
                    Console.Beep(6200, 50);
                    Console.Beep(6200, 50);
                    _isOk(res);

                }

                else
                {
                    Console.Beep(2200, 200);
                    Console.Beep(1200, 200);
                    _isOk(new AltsInfo { Authenticated = false, AuthDeviceId = "Not Trusted" });
                }
                
            }

            else
            {
                
                Console.Out.WriteLine("trainMode On");
                TrustDevice(info.ReaderName);
                TrainMode = false;
            }
        }

        private void TrustDevice(string readerName)
        {
            SmartCard sa = new SmartCard(readerName);
            var res = sa.GetAIDs(true);
            SmartApplication sapp = sa.Applications[0];
            sapp.SelectApplication();
            sapp.GetProcessingOptions();
            sapp.ReadRecords();
            OfflineAuth oa = new OfflineAuth(sapp);

            string cardNumber = sapp.GetTagValue(EmvConstants.ResponceType.ReaderRecord, "5A");
            string expdate = sapp.GetTagValue(EmvConstants.ResponceType.ReaderRecord, "5F25");
            ///Keep the hash data of cardnumber and expiryDate
            string token = HashData(cardNumber + expdate, oa.ICC_KEY_HASH);

            AltsInfo ok = new AltsInfo() { AuthDeviceId = token ,Authenticated = !string.IsNullOrEmpty(token) };
            if (ok.Authenticated)
                TrustedDevice = ok;
        }

        /// <summary>
        /// Read the smart card and authenticate it using EMVCO rules
        /// </summary>
        /// <param name="readerName"></param>
        /// <returns>AltsInfo Authenticated True or false</returns>
        private AltsInfo Authenticate(string readerName)
        {
            AltsInfo ok = new AltsInfo();
            try
            {
                SmartCard sa = new SmartCard(readerName);
                var res = sa.GetAIDs(true);
                SmartApplication sapp = sa.Applications[0];
                sapp.SelectApplication();
                sapp.GetProcessingOptions();
                sapp.ReadRecords();


                OfflineAuth oa = new OfflineAuth(sapp);

                string cardNumber = sapp.GetTagValue(EmvConstants.ResponceType.ReaderRecord, "5A");
                string expdate = sapp.GetTagValue(EmvConstants.ResponceType.ReaderRecord, "5F25");
                ///Keep the hash data of cardnumber and expiryDate
                string token = HashData(cardNumber + expdate, oa.ICC_KEY_HASH);

                ok.Authenticated = !string.IsNullOrEmpty(token);
                ok.AuthDeviceId = token;
            }
            catch (Exception ex)
            {
                ok.Authenticated = false;
                ok.AuthDeviceId = ex.Message;
            }
            return ok;
        }

        /// <summary>
        /// StatusChanged, only logs to console
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void StatusChanged(object sender, StatusChangeEventArgs args)
        {
            Console.WriteLine(">> StatusChanged Event for reader: {0}", args.ReaderName);
            Console.WriteLine("ATR: {0}", BitConverter.ToString(args.Atr ?? new byte[0]));
            Console.WriteLine("Last state: {0}\nNew state: {1}\n", args.LastState, args.NewState);
        }

        /// <summary>
        /// Logs Exceptions to Console
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ex"></param>
        private static void MonitorException(object sender, PCSCException ex)
        {
            Console.WriteLine("Monitor exited due an error:");
            Console.WriteLine(SCardHelper.StringifyError(ex.SCardError));
        }


        /// <summary>
        /// Gets installed smart card reader names
        /// </summary>
        /// <returns>Array of reader names installed</returns>
        private string[] GetReaderNames()
        {
            using (var context = _contextFactory.Establish(SCardScope.System))
            {
                return context.GetReaders();
            }
        }

        /// <summary>
        /// Check if reader is found
        /// </summary>
        /// <param name="readerNames"></param>
        /// <returns>True if no readers are found</returns>
        private static bool NoReaderFound(ICollection<string> readerNames)
        {
            return readerNames == null || readerNames.Count < 1;
        }


        /// <summary>
        /// Check AltsInfo if the card is trusted or not
        /// </summary>
        /// <param name="devToken"></param>
        /// <returns>True for trusted smart card</returns>
        public bool isTrusted(AltsInfo devToken) {
            if (TrustedDevice == null)
                return false;
            if (TrustedDevice.Authenticated == false || string.IsNullOrEmpty(TrustedDevice.AuthDeviceId))
                return false;


            if (devToken == null)
                return false;
            if (devToken.Authenticated == false || string.IsNullOrEmpty(devToken.AuthDeviceId))
                return false;

            if (devToken.Authenticated && TrustedDevice.Authenticated)
                return devToken.AuthDeviceId == TrustedDevice.AuthDeviceId;

            return false;

        }

        /// <summary>
        /// Function to train the application to trust a specified card
        /// </summary>
        /// <param name="isTrusted"></param>
        public async void TrustDevice(CallBacks.Trusted isTrusted)
        {
            _isTrusted = isTrusted;
            await Task.Factory.StartNew(() =>
            {
                TrainMode = true;
                
                for(int i =0; i<10; i++)
                {
                    Task.Delay(1000).Wait();
                    Console.Beep(5100, 200);
                    if (!TrainMode)
                    {

                        break;
                    }
                }
                if (TrustedDevice == null)
                {
                    TrustedDevice = new AltsInfo() { Authenticated = false, AuthDeviceId = null };
                }
                _isTrusted(TrustedDevice);
                TrainMode = false;
            });
            
        }

        /// <summary>
        /// Function to Untrust device. Nor yet implemented.
        /// </summary>
        /// <param name="devInfo"></param>
        public void UnTrustDevice(AltsInfo devInfo)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a hash value to store as the TrustedDevice token
        /// </summary>
        /// <param name="dataToHash"></param>
        /// <param name="saltValue"></param>
        /// <returns>Hash value of the cardnumber,expiry date and saltValue (ICC publicKey hash is used)</returns>
        private static string HashData(string dataToHash, byte[] saltValue)
        {
            var hashAlg = System.Security.Cryptography.HashAlgorithm.Create("SHA256");
            List<byte> inputdata = new List<byte>();
            inputdata.AddRange(StringTools.HexStringToByteArray(dataToHash));
            inputdata.AddRange(saltValue);
            string finaldata = StringTools.ByteArrayToHexString(saltValue);
            finaldata += StringTools.ByteArrayToHexString(hashAlg.ComputeHash(inputdata.ToArray()));
            return finaldata;
        }

    }
}
