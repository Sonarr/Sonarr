using System;

namespace NzbDrone.Core.Providers
{
    public interface IConfigProvider
    {
        String SeriesRoot { get; set; }

        string GetValue(string key, object defaultValue, bool makePermanent);
        void SetValue(string key, string value);
    }
}