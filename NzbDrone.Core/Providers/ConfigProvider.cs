using System;
using log4net;
using NzbDrone.Core.Repository;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers
{
    public class ConfigProvider : IConfigProvider
    {
        private const string SERIES_ROOTS = "SeriesRoots";
        private readonly ILog _logger;
        private readonly IRepository _sonicRepo;


        public ConfigProvider(ILog logger, IRepository dataRepository)
        {
            _logger = logger;

            _sonicRepo = dataRepository;
        }


        private string GetValue(string key)
        {
            return GetValue(key, String.Empty, false);
        }

        public String SeriesRoot
        {
            get
            {
                return GetValue(SERIES_ROOTS);
            }

            set
            {
                SetValue(SERIES_ROOTS, value);
            }

        }


        public string GetValue(string key, object defaultValue, bool makePermanent)
        {
            string value;

            var dbValue = _sonicRepo.Single<Config>(key);

            if (dbValue != null && !String.IsNullOrEmpty(dbValue.Value))
            {
                return dbValue.Value;
            }


            _logger.WarnFormat("Unable to find config key '{0}' defaultValue:'{1}'", key, defaultValue);
            if (makePermanent)
            {
                SetValue(key, defaultValue.ToString());
            }
            value = defaultValue.ToString();


            return value;
        }

        public void SetValue(string key, string value)
        {
            if (String.IsNullOrEmpty(key)) throw new ArgumentOutOfRangeException("key");
            if (value == null) throw new ArgumentNullException("key");

            _logger.DebugFormat("Writing Setting to file. Key:'{0}' Value:'{1}'", key, value);

            var dbValue = _sonicRepo.Single<Config>(key);

            if (dbValue == null)
            {
                _sonicRepo.Add(new Config { Key = key, Value = value });
            }
            else
            {
                dbValue.Value = value;
                _sonicRepo.Update(dbValue);
            }
        }
    }
}