using System.Text;
using Nancy;

namespace NzbDrone.Api.Profiles
{
    class LegacyProfileModule : NzbDroneApiModule
    {
        public LegacyProfileModule()
            : base("qualityprofile")
        {
            Get["/"] = x =>
            {
                string queryString = ConvertQueryParams(Request.Query);
                var url = string.Format("/api/profile?{0}", queryString);

                return Response.AsRedirect(url);
            };
        }

        private string ConvertQueryParams(DynamicDictionary query)
        {
            var sb = new StringBuilder();

            foreach (var key in query)
            {
                var value = query[key];

                sb.AppendFormat("&{0}={1}", key, value);
            }

            return sb.ToString().Trim('&');
        }
    }
}
