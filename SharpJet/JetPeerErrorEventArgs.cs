using System;
using Newtonsoft.Json.Linq;

namespace Hbm.Devices.Jet
{
    public class JetPeerErrorEventArgs : EventArgs
    {
        public StatusCode StatusCode { get; }
        public JObject Json { get; }

        public JetPeerErrorEventArgs(StatusCode statusCode, JObject json)
        {
            StatusCode = statusCode;
            Json = json;
        }
    }
}
