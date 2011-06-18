using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using PetaPoco;

namespace NzbDrone.Core.Providers
{
    public class SceneMappingProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IDatabase _database;
        private readonly HttpProvider _httpProvider;

        public SceneMappingProvider(IDatabase database, HttpProvider httpProvider)
        {
            _database = database;
            _httpProvider = httpProvider;
        }

        public SceneMappingProvider()
        {
            
        }

        public virtual bool UpdateMappings()
        {
            try
            {
                var mapping = _httpProvider.DownloadString("http://vps.nzbdrone.com/SceneMappings.csv");
                var newMaps = new List<SceneMapping>();

                using (var reader = new StringReader(mapping))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var split = line.Split(',');
                        var seriesId = 0;
                        Int32.TryParse(split[1], out seriesId);

                        var map = new SceneMapping();
                        map.CleanTitle = split[0];
                        map.SeriesId = seriesId;
                        map.SceneName = split[2];

                        newMaps.Add(map);
                    }
                }

                Logger.Debug("Deleting all existing Scene Mappings.");
                _database.Delete<SceneMapping>(String.Empty);

                Logger.Debug("Adding Scene Mappings");
                _database.InsertMany(newMaps);
            }

            catch (Exception ex)
            {
                Logger.InfoException("Failed to Update Scene Mappings", ex);
                return false;
            }
            return true;
        }

        public virtual List<SceneMapping> GetAll()
        {
            return _database.Fetch<SceneMapping>();
        }

        public virtual string GetSceneName(int seriesId)
        {
            var item = _database.SingleOrDefault<SceneMapping>("WHERE SeriesId = @0", seriesId);

            if (item == null)
                return null;

            return item.SceneName;
        }

        public virtual Nullable<Int32> GetSeriesId(string cleanName)
        {
            var item = _database.SingleOrDefault<SceneMapping>("WHERE CleanTitle = @0", cleanName);

            if (item == null)
                return null;

            return item.SeriesId;
        }
    }
}
