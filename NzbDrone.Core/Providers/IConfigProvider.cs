using System;

namespace NzbDrone.Core.Providers
{
    public interface IConfigProvider
    {
        String SeriesRoot { get; set; }
        String EpisodeNameFormat { get; set; }
        String NzbMatrixUsername { get; set; }
        String NzbMatrixApiKey { get; set; }
        String NzbsOrgUId { get; set; }
        String NzbsOrgHash { get; set; }
        String NzbsrusUId { get; set; }
        String NzbsrusHash { get; set; }
        String DownloadPropers { get; set; }
        String Retention { get; set; }
        String SabHost { get; set; }
        String SabPort { get; set; }
        String SabApiKey { get; set; }
        String SabUsername { get; set; }
        String SabPassword { get; set; }
        String SabTvCategory { get; set; }
        String UseBlackhole { get; set; }
        String BlackholeDirectory { get; set; }
        String SyncFrequency { get; set; }
        String SabTvPriority { get; set; }
        String ApiKey { get; set; }

        string GetValue(string key, object defaultValue, bool makePermanent);
        void SetValue(string key, string value);
    }
}