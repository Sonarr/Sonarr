using System;
using NLog;
using NzbDrone.Core.Repository;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers.Core
{
    public class ConfigProvider : IConfigProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IRepository _sonicRepo;

        public ConfigProvider(IRepository dataRepository)
        {
            _sonicRepo = dataRepository;
        }

        public String ApiKey
        {
            get { return GetValue("ApiKey"); }

            set { SetValue("ApiKey", value); }
        }


        public String EpisodeNameFormat
        {
            get { return GetValue("EpisodeNameFormat"); }

            set { SetValue("EpisodeNameFormat", value); }
        }

        public String SeriesRoot
        {
            get { return GetValue("SeriesRoots"); }

            set { SetValue("SeriesRoots", value); }
        }

        public String NzbMatrixUsername
        {
            get { return GetValue("NzbMatrixUsername"); }

            set { SetValue("NzbMatrixUsername", value); }
        }

        public String NzbMatrixApiKey
        {
            get { return GetValue("NzbMatrixApiKey"); }

            set { SetValue("NzbMatrixApiKey", value); }
        }

        public String NzbsOrgUId
        {
            get { return GetValue("NzbsOrgUId"); }

            set { SetValue("NzbsOrgUId", value); }
        }

        public String NzbsOrgHash
        {
            get { return GetValue("NzbsOrgHash"); }

            set { SetValue("NzbsOrgHash", value); }
        }

        public String NzbsrusUId
        {
            get { return GetValue("NzbsrusUId"); }

            set { SetValue("NzbsrusUId", value); }
        }

        public String NzbsrusHash
        {
            get { return GetValue("NzbsrusHash"); }

            set { SetValue("NzbsrusHash", value); }
        }

        public String SyncFrequency
        {
            get { return GetValue("SyncFrequency"); }

            set { SetValue("SyncFrequency", value); }
        }

        public String DownloadPropers
        {
            get { return GetValue("DownloadPropers"); }

            set { SetValue("DownloadPropers", value); }
        }

        public String Retention
        {
            get { return GetValue("Retention"); }

            set { SetValue("Retention", value); }
        }

        public String SabHost
        {
            get { return GetValue("SabHost"); }

            set { SetValue("SabHost", value); }
        }

        public String SabPort
        {
            get { return GetValue("SabPort"); }

            set { SetValue("SabPort", value); }
        }

        public String SabApiKey
        {
            get { return GetValue("SabApiKey"); }

            set { SetValue("SabApiKey", value); }
        }

        public String SabUsername
        {
            get { return GetValue("SabUsername"); }

            set { SetValue("SabUsername", value); }
        }

        public String SabPassword
        {
            get { return GetValue("SabPassword"); }

            set { SetValue("SabPassword", value); }
        }

        public String SabTvCategory
        {
            get { return GetValue("SabTvCategory"); }

            set { SetValue("SabTvCategory", value); }
        }

        public String SabTvPriority
        {
            get { return GetValue("SabTvPriority"); }

            set { SetValue("SabTvPriority", value); }
        }

        public String UseBlackhole
        {
            get { return GetValue("UseBlackhole"); }

            set { SetValue("UseBlackhole", value); }
        }

        public String BlackholeDirectory
        {
            get { return GetValue("BlackholeDirectory"); }

            set { SetValue("BlackholeDirectory", value); }
        }

        public bool UseSeasonFolder
        {
            get { return GetValueBoolean("Sorting_SeasonFolder", true); }

            set { SetValue("Sorting_SeasonFolder", value); }
        }

        public int DefaultQualityProfile
        {
            get { return GetValueInt("DefaultQualityProfile", 1); }

            set { SetValue("DefaultQualityProfile", value); }
        }


        private string GetValue(string key)
        {
            return GetValue(key, String.Empty, false);
        }

        private bool GetValueBoolean(string key, bool defaultValue = false)
        {
            return Convert.ToBoolean(GetValue(key, defaultValue, false));
        }

        private int GetValueInt(string key, int defaultValue = 0)
        {
            return Convert.ToInt16(GetValue(key, defaultValue, false));
        }

        public string GetValue(string key, object defaultValue, bool makePermanent)
        {
            string value;

            var dbValue = _sonicRepo.Single<Config>(key);

            if (dbValue != null && !String.IsNullOrEmpty(dbValue.Value))
                return dbValue.Value;

            Logger.Debug("Unable to find config key '{0}' defaultValue:'{1}'", key, defaultValue);
            if (makePermanent)
                SetValue(key, defaultValue.ToString());
            value = defaultValue.ToString();

            return value;
        }

        public void SetValue(string key, Boolean value)
        {
            SetValue(key, value.ToString());
        }

        public void SetValue(string key, int value)
        {
            SetValue(key, value.ToString());
        }

        public void SetValue(string key, string value)
        {
            if (String.IsNullOrEmpty(key))
                throw new ArgumentOutOfRangeException("key");
            if (value == null)
                throw new ArgumentNullException("key");

            Logger.Debug("Writing Setting to file. Key:'{0}' Value:'{1}'", key, value);

            var dbValue = _sonicRepo.Single<Config>(key);

            if (dbValue == null)
            {
                _sonicRepo.Add(new Config
                {
                    Key = key,
                    Value = value
                });
            }
            else
            {
                dbValue.Value = value;
                _sonicRepo.Update(dbValue);
            }
        }
    }
}