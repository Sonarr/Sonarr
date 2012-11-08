namespace NzbDrone.Core.Model
{
    public class CredentialInfoModel
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public bool HasCredentials
        {
            get { return string.IsNullOrEmpty(Username) == false && string.IsNullOrEmpty(Password) == false; }
        }
    }

    public class ConnectionInfoModel
    {
        public string Address { get; set; }
        public int Port { get; set; }

        public CredentialInfoModel Credentials { get; set; }

        public bool HasCredentials
        {
            get { if (Credentials == null) return false; return Credentials.HasCredentials; }
        }

    }
}
