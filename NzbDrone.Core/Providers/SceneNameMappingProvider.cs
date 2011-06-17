using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using NLog;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers
{
    public class SceneNameMappingProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IRepository _repository;
        private readonly HttpProvider _httpProvider;

        public SceneNameMappingProvider(IRepository repository, HttpProvider httpProvider)
        {
            _repository = repository;
            _httpProvider = httpProvider;
        }

        public SceneNameMappingProvider()
        {
            
        }

        public virtual bool UpdateMappings()
        {
            try
            {
                var mapping = _httpProvider.DownloadString("http://vps.nzbdrone.com/SceneNameMappings.csv");
                var newMaps = new List<SceneNameMapping>();

                using (var reader = new StringReader(mapping))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var split = line.Split(',');
                        var seriesId = 0;
                        Int32.TryParse(split[1], out seriesId);

                        var map = new SceneNameMapping();
                        map.SceneCleanName = split[0];
                        map.SeriesId = seriesId;
                        map.SceneName = split[2];

                        newMaps.Add(map);
                    }
                }

                Logger.Debug("Deleting all existing Scene Mappings.");
                _repository.DeleteMany<SceneNameMapping>(GetAll());

                Logger.Debug("Adding Scene Mappings");
                _repository.AddMany(newMaps);
            }

            catch (Exception ex)
            {
                Logger.InfoException("Failed to Update Scene Mappings", ex);
                return false;
            }
            return true;
        }

        public virtual List<SceneNameMapping> GetAll()
        {
            return _repository.All<SceneNameMapping>().ToList();
        }

        public virtual string GetSceneName(int seriesId)
        {
            var item = _repository.Single<SceneNameMapping>(s => s.SeriesId == seriesId);

            if (item == null)
                return null;

            return item.SceneName;
        }

        public virtual Nullable<Int32> GetSeriesId(string cleanName)
        {
            var item  = _repository.Single<SceneNameMapping>(s => s.SceneCleanName == cleanName);

            if (item == null)
                return null;

            return item.SeriesId;
        }
    }
}
