using System.Collections.Generic;

namespace NzbDrone.Core.DataAugmentation.Scene
{
    public class ServicesProvider : ISceneMappingProvider
    {
        private readonly ISceneMappingProxy _sceneMappingProxy;

        public ServicesProvider(ISceneMappingProxy sceneMappingProxy)
        {
            _sceneMappingProxy = sceneMappingProxy;
        }

        public List<SceneMapping> GetSceneMappings()
        {
            return _sceneMappingProxy.Fetch();
        }
    }
}
