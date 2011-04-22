using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Repository;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers.Core
{
    public class ConfigProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IRepository _sonicRepo;

        public ConfigProvider(IRepository dataRepository)
        {
            _sonicRepo = dataRepository;
        }

        public IList<Config> All()
        {
            return _sonicRepo.All<Config>().ToList();
        }

        public ConfigProvider()
        {
        }

        public virtual String ApiKey
        {
            get { return GetValue("ApiKey"); }

            set { SetValue("ApiKey", value); }
        }

        public virtual String EpisodeNameFormat
        {
            get { return GetValue("EpisodeNameFormat"); }

            set { SetValue("EpisodeNameFormat", value); }
        }

        public virtual String SeriesRoot
        {
            get { return GetValue("SeriesRoots"); }

            set { SetValue("SeriesRoots", value); }
        }

        public virtual String NzbMatrixUsername
        {
            get { return GetValue("NzbMatrixUsername"); }

            set { SetValue("NzbMatrixUsername", value); }
        }

        public virtual String NzbMatrixApiKey
        {
            get { return GetValue("NzbMatrixApiKey"); }

            set { SetValue("NzbMatrixApiKey", value); }
        }

        public virtual String NzbsOrgUId
        {
            get { return GetValue("NzbsOrgUId"); }

            set { SetValue("NzbsOrgUId", value); }
        }

        public virtual String NzbsOrgHash
        {
            get { return GetValue("NzbsOrgHash"); }

            set { SetValue("NzbsOrgHash", value); }
        }

        public virtual String NzbsrusUId
        {
            get { return GetValue("NzbsrusUId"); }

            set { SetValue("NzbsrusUId", value); }
        }

        public virtual String NzbsrusHash
        {
            get { return GetValue("NzbsrusHash"); }

            set { SetValue("NzbsrusHash", value); }
        }

        public virtual String NewzbinUsername
        {
            get { return GetValue("NewzbinUsername"); }

            set { SetValue("NewzbinUsername", value); }
        }

        public virtual String NewzbinPassword
        {
            get { return GetValue("NewzbinPassword"); }

            set { SetValue("NewzbinPassword", value); }
        }

        public virtual String SyncFrequency
        {
            get { return GetValue("SyncFrequency"); }

            set { SetValue("SyncFrequency", value); }
        }

        public virtual String DownloadPropers
        {
            get { return GetValue("DownloadPropers"); }

            set { SetValue("DownloadPropers", value); }
        }

        public virtual String Retention
        {
            get { return GetValue("Retention"); }

            set { SetValue("Retention", value); }
        }

        public virtual String SabHost
        {
            get { return GetValue("SabHost"); }

            set { SetValue("SabHost", value); }
        }

        public virtual String SabPort
        {
            get { return GetValue("SabPort"); }

            set { SetValue("SabPort", value); }
        }

        public virtual String SabApiKey
        {
            get { return GetValue("SabApiKey"); }

            set { SetValue("SabApiKey", value); }
        }

        public virtual String SabUsername
        {
            get { return GetValue("SabUsername"); }

            set { SetValue("SabUsername", value); }
        }

        public virtual String SabPassword
        {
            get { return GetValue("SabPassword"); }

            set { SetValue("SabPassword", value); }
        }

        public virtual String SabTvCategory
        {
            get { return GetValue("SabTvCategory"); }

            set { SetValue("SabTvCategory", value); }
        }

        public virtual String SabTvPriority
        {
            get { return GetValue("SabTvPriority"); }

            set { SetValue("SabTvPriority", value); }
        }

        public virtual String UseBlackhole
        {
            get { return GetValue("UseBlackhole"); }

            set { SetValue("UseBlackhole", value); }
        }

        public virtual String BlackholeDirectory
        {
            get { return GetValue("BlackholeDirectory"); }

            set { SetValue("BlackholeDirectory", value); }
        }

        public virtual bool UseSeasonFolder
        {
            get { return GetValueBoolean("Sorting_SeasonFolder", true); }

            set { SetValue("Sorting_SeasonFolder", value); }
        }

        public virtual int DefaultQualityProfile
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

        public virtual string GetValue(string key, object defaultValue, bool makePermanent)
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

        public virtual void SetValue(string key, Boolean value)
        {
            SetValue(key, value.ToString());
        }

        public virtual void SetValue(string key, int value)
        {
            SetValue(key, value.ToString());
        }

        public virtual void SetValue(string key, string value)
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