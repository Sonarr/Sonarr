namespace Sonarr.Http.Extensions
{
    public static class AccessControlHeaders
    {
        public const string RequestMethod = "Access-Control-Request-Method";
        public const string RequestHeaders = "Access-Control-Request-Headers";

        public const string AllowOrigin = "Access-Control-Allow-Origin";
        public const string AllowMethods = "Access-Control-Allow-Methods";
        public const string AllowHeaders = "Access-Control-Allow-Headers";
    }
}
