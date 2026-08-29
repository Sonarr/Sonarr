namespace Sonarr.Http.Authentication
{
    public class ResetPasswordResource
    {
        public string Token { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string PasswordConfirmation { get; set; }
    }
}
