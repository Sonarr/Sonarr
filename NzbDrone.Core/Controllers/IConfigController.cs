using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Controllers
{
    public interface IConfigController
    {
        String SeriesRoot
        {
            get;

            set;
        }
    }
}