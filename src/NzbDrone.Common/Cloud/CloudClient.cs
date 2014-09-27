using NzbDrone.Common.Http;

namespace NzbDrone.Common.Cloud
{
    public interface IDroneServicesRequestBuilder
    {
        HttpRequest Build(string path);
    }

    public class DroneServicesHttpRequestBuilder : HttpRequestBuilder, IDroneServicesRequestBuilder
    {
        private const string ROOT_URL = "http://services.nzbdrone.com/v1/";

        public DroneServicesHttpRequestBuilder()
            : base(ROOT_URL)
        {
        }
    }
}
