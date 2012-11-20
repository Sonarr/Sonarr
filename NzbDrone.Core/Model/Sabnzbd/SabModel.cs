namespace NzbDrone.Core.Model.Sabnzbd
{
    public class SabModel
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string ApiKey { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
