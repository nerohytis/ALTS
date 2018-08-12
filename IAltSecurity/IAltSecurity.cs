using System;

namespace InterfaceAltSecurity
{
    public interface IAltSecurity
    {

        void Connect(CallBacks.Authenticated AuthCallBack);
        void Disconnect();

        void TrustDevice(CallBacks.Trusted TrustedCallback);
        void UnTrustDevice(AltsInfo devInfo);

    }

    public class CallBacks {
        public delegate void Authenticated(AltsInfo result);
        public delegate void Trusted(AltsInfo result);
        //public delegate void EndStatus(AltsInfo result);
    }


    public class AltsInfo {
        public string AuthDeviceId { get; set; }
        public bool Authenticated { get; set; }
        public int AuthDuration { get; set; }

    }

}
