using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using Newtonsoft.Json;
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
                const string url = "http://services.nzbdrone.com/SceneMapping/Active";
                
                var mappingsJson = _httpProvider.DownloadString(url);
                var mappings = JsonConvert.DeserializeObject<List<SceneMapping>>(mappingsJson);

                Logger.Debug("Deleting all existing Scene Mappings.");
                _database.Delete<SceneMapping>(String.Empty);

                Logger.Debug("Adding Scene Mappings");
                _database.InsertMany(mappings);
            }

            catch (Exception ex)
            {
                Logger.InfoException("Failed to Update Scene Mappings:", ex);
                return false;
            }
            return true;
        }

        public virtual string GetSceneName(int seriesId)
        {
            UpdateIfEmpty();

            var item = _database.FirstOrDefault<SceneMapping>("WHERE SeriesId = @0", seriesId);

            if (item == null)
                return null;

            return item.SceneName;
        }

        public virtual Nullable<Int32> GetSeriesId(string cleanName)
        {
            UpdateIfEmpty();

            var item = _database.SingleOrDefault<SceneMapping>("WHERE CleanTitle = @0", cleanName);

            if (item == null)
                return null;

            return item.SeriesId;
        }

        public void UpdateIfEmpty()
        {
            var count = _database.ExecuteScalar<int>("SELECT COUNT(*) FROM SceneMappings");

            if (count == 0)
                UpdateMappings();
        }
    }
}
