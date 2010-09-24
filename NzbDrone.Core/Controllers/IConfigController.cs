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

        string GetValue(string key, object defaultValue, bool makePermanent);
        void SetValue(string key, string value);
    }
}