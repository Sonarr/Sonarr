using System.Collections.Generic;

namespace NzbDrone.Core.DataAugmentation.Scene
{
    public interface ISceneMappingProvider
    {
        List<SceneMapping> GetSceneMappings();
    }
}
