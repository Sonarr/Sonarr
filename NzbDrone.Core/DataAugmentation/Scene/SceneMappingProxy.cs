using System.Collections.Generic;
using NzbDrone.Common;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.DataAugmentation.Scene
{
    public interface ISceneMappingProxy
    {
        List<SceneMapping> Fetch();
    }

    public class SceneMappingProxy : ISceneMappingProxy
    {
        private readonly IHttpProvider _httpProvider;
        private readonly IConfigService _configService;
        

        public SceneMappingProxy(IHttpProvider httpProvider, IConfigService configService)
        {
            _httpProvider = httpProvider;
            _configService = configService;
            
        }

        public List<SceneMapping> Fetch()
        {
            var mappingsJson = _httpProvider.DownloadString(_configService.ServiceRootUrl + "/SceneMapping/Active");
            return Json.Deserialize<List<SceneMapping>>(mappingsJson);
        }
    }
}