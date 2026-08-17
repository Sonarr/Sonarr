using RestSharp;
using Sonarr.Api.V5.Series;

namespace NzbDrone.Integration.Test.Client
{
    public class SeriesClientV5 : ClientBase<SeriesResource>
    {
        public SeriesClientV5(IRestClient restClient, string apiKey)
            : base(restClient, apiKey)
        {
        }

        public SeriesResource Put(SeriesResource series, bool moveFiles)
        {
            var request = BuildRequest();
            request.AddQueryParameter("moveFiles", moveFiles.ToString());
            request.AddJsonBody(series);
            return Put<SeriesResource>(request);
        }
    }
}
