using System.Collections.Generic;
using NzbDrone.Common;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.DataAugmentation.Scene
{
    public interface ISceneMappingProxy
    {
        List<SceneMapping> Fetch();
    }

    public class SceneMappingProxy : ISceneMappingProxy
    {
        private readonly IHttpProvider _httpProvider;

        public SceneMappingProxy(IHttpProvider httpProvider)
        {
            _httpProvider = httpProvider;
        }

        public List<SceneMapping> Fetch()
        {
            var mappingsJson = _httpProvider.DownloadString(Services.RootUrl + "/v1/SceneMapping");
            return Json.Deserialize<List<SceneMapping>>(mappingsJson);
        }
    }
}