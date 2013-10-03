using System;

namespace NzbDrone.Core.Download.Clients.Nzbget
{
    public class ErrorModel
    {
        public String Name { get; set; }
        public Int32 Code { get; set; }
        public String Message { get; set; }

        public override string ToString()
        {
            return String.Format("Name: {0}, Code: {1}, Message: {2}", Name, Code, Message);
        }
    }
}
