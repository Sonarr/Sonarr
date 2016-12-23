namespace NzbDrone.Core.Download.Clients.Nzbget
{
    public class ErrorModel
    {
        public string Name { get; set; }
        public int Code { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            return string.Format("Name: {0}, Code: {1}, Message: {2}", Name, Code, Message);
        }
    }
}
