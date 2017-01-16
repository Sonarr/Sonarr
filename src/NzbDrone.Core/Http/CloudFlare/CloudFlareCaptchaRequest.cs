using NzbDrone.Common.Http;

namespace NzbDrone.Core.Http.CloudFlare
{
    public class CloudFlareCaptchaRequest
    {
        public string Host { get; set; }
        public string SiteKey { get; set; }

        public string Ray { get; set; }
        public string SecretToken { get; set; }

        public HttpUri ResponseUrl { get; set; }
    }
}
