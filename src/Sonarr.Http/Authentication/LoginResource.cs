namespace Sonarr.Http.Authentication
{
    public class LoginResource
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
