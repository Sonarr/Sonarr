using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers.Core
{
    public class ConfigProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IRepository _repository;

        [Inject]
        public ConfigProvider(IRepository repository)
        {
            _repository = repository;
        }

        public IList<Config> All()
        {
            return _repository.All<Config>().ToList();
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

        public virtual int SyncFrequency
        {
            get { return GetValueInt("SyncFrequency"); }

            set { SetValue("SyncFrequency", value); }
        }

        public virtual Boolean DownloadPropers
        {
            get { return GetValueBoolean("DownloadPropers"); }

            set { SetValue("DownloadPropers", value); }
        }

        public virtual Int32 Retention
        {
            get { return GetValueInt("Retention"); }

            set { SetValue("Retention", value); }
        }

        public virtual String SabHost
        {
            get { return GetValue("SabHost", "localhost", true); }

            set { SetValue("SabHost", value); }
        }

        public virtual int SabPort
        {
            get { return GetValueInt("SabPort", 8080); }

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
            get { return GetValue("SabTvCategory", "TV", false); }

            set { SetValue("SabTvCategory", value); }
        }

        public virtual SabnzbdPriorityType SabTvPriority
        {
            get { return (SabnzbdPriorityType)GetValueInt("SabTvPriority"); }

            set { SetValue("SabTvPriority", (int)value); }
        }

        public virtual String SabDropDirectory
        {
            get { return GetValue("SabTvDropDirectory", "", false); }

            set { SetValue("SabTvDropDirectory", value); }
        }

        public virtual Boolean UseBlackhole
        {
            get { return GetValueBoolean("UseBlackhole"); }

            set { SetValue("UseBlackhole", value); }
        }

        public virtual String BlackholeDirectory
        {
            get { return GetValue("BlackholeDirectory"); }

            set { SetValue("BlackholeDirectory", value); }
        }

        public virtual bool SeriesName
        {
            get { return GetValueBoolean("Sorting_SeriesName", true); }
            set { SetValue("Sorting_SeriesName", value); }
        }

        public virtual bool EpisodeName
        {
            get { return GetValueBoolean("Sorting_EpisodeName", true); }
            set { SetValue("Sorting_EpisodeName", value); }
        }

        public virtual bool ReplaceSpaces
        {
            get { return GetValueBoolean("Sorting_ReplaceSpaces", true); }
            set { SetValue("Sorting_ReplaceSpaces", value); }
        }

        public virtual bool AppendQuality
        {
            get { return GetValueBoolean("Sorting_AppendQaulity", true); }
            set { SetValue("Sorting_AppendQaulity", value); }
        }

        public virtual bool UseSeasonFolder
        {
            get { return GetValueBoolean("Sorting_SeasonFolder", true); }

            set { SetValue("Sorting_SeasonFolder", value); }
        }

        public virtual string SeasonFolderFormat
        {
            get { return GetValue("Sorting_SeasonFolderFormat", "Season %s", false); }
            set { SetValue("Sorting_SeasonFolderFormat", value); }
        }

        public virtual int SeparatorStyle
        {
            get { return GetValueInt("Sorting_SeparatorStyle"); }
            set { SetValue("Sorting_SeparatorStyle", value); }
        }

        public virtual int NumberStyle
        {
            get { return GetValueInt("Sorting_NumberStyle", 2); }
            set { SetValue("Sorting_NumberStyle", value); }
        }

        public virtual int MultiEpisodeStyle
        {
            get { return GetValueInt("Sorting_MultiEpisodeStyle"); }
            set { SetValue("Sorting_MultiEpisodeStyle", value); }
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

            var dbValue = _repository.Single<Config>(key);

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

            var dbValue = _repository.Single<Config>(key);

            if (dbValue == null)
            {
                _repository.Add(new Config
                                   {
                                       Key = key,
                                       Value = value
                                   });
            }
            else
            {
                dbValue.Value = value;
                _repository.Update(dbValue);
            }
        }
    }
}