using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Controllers
{
    public interface IHttpController
    {
        string GetRequest(string request);
    }
}
