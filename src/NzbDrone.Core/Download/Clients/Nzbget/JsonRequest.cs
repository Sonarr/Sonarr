using System;

namespace NzbDrone.Core.Download.Clients.Nzbget
{
    public class JsonRequest
    {
        public String Method { get; set; }
        public object[] Params { get; set; }

        public JsonRequest(string method)
        {
            Method = method;
        }

        public JsonRequest(string method, object[] @params)
        {
            Method = method;
            Params = @params;
        }
    }
}
