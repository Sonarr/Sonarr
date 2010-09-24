using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using NzbDrone.Core.Repository;
using SubSonic.Repository;

namespace NzbDrone.Core.Controllers
{
    public class DbConfigController : IConfigController
    {
        private const string _seriesroots = "SeriesRoots";
        private readonly IDiskController _diskController;
        private readonly ILog _logger;
        private readonly IRepository _sonicRepo;


        public DbConfigController(ILog logger, IDiskController diskController, IRepository dataRepository)
        {
            _logger = logger;
            _diskController = diskController;
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
                return GetValue(_seriesroots);
            }

            set
            {
                SetValue(_seriesroots, value);
            }

        }


        private string GetValue(string key, object defaultValue, bool makePermanent)
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

        private void SetValue(string key, string value)
        {
            _logger.DebugFormat("Writing Setting to file. Key:'{0}' Value:'{1}'", key, value);

            _sonicRepo.Add(new Config { Key = key, Value = value });
        }
    }
}