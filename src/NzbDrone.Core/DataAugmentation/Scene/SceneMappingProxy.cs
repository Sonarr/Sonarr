using System.Collections.Generic;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.DataAugmentation.Scene
{
    public interface ISceneMappingProxy
    {
        List<SceneMapping> Fetch();
    }

    public class SceneMappingProxy : ISceneMappingProxy
    {
        private readonly IHttpClient _httpClient;
        private readonly IDroneServicesRequestBuilder _requestBuilder;

        public SceneMappingProxy(IHttpClient httpClient, IDroneServicesRequestBuilder requestBuilder)
        {
            _httpClient = httpClient;
            _requestBuilder = requestBuilder;
        }

        public List<SceneMapping> Fetch()
        {
            var request = _requestBuilder.Build("/scenemapping");
            return _httpClient.Get<List<SceneMapping>>(request).Resource;
        }
    }
}