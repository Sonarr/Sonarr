namespace NzbDrone.Host.AccessControl
{
    public class UrlAcl
    {
        public string Scheme { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }
        public string UrlBase { get; set; }

        public string Url => string.Format("{0}://{1}:{2}/{3}", Scheme, Address, Port, UrlBase);
    }
}
