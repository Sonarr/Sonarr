using System.Collections.Generic;

namespace NzbDrone.Core.Controllers
{
    public interface IConfigController
    {
        List<string> GetTvRoots();
    }
}