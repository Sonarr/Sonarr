namespace NzbDrone.Core.Notifications.Join
{
    public class JoinResponseModel
    {
        public bool success { get; set; }
        public bool userAuthError { get; set; }
        public string errorMessage { get; set; }
        public string kind { get; set; }
        public string etag { get; set; }
    }
}
