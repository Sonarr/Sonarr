using System.Text;
using Nancy;

namespace NzbDrone.Api.Wanted
{
    class LegacyMissingModule : NzbDroneApiModule
    {
        public LegacyMissingModule() : base("missing")
        {
            Get["/"] = x =>
            {
                string queryString = ConvertQueryParams(Request.Query);
                var url = string.Format("/api/wanted/missing?{0}", queryString);

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
