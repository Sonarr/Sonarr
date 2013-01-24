using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using Newtonsoft.Json;
using NzbDrone.Common;
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
        private readonly ConfigProvider _configProvider;

        public SceneMappingProvider(IDatabase database, HttpProvider httpProvider, ConfigProvider configProvider)
        {
            _database = database;
            _httpProvider = httpProvider;
            _configProvider = configProvider;
        }

        public SceneMappingProvider()
        {

        }

        public virtual bool UpdateMappings()
        {
            try
            {
                var mappingsJson = _httpProvider.DownloadString(_configProvider.ServiceRootUrl + "/SceneMapping/Active");
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

        public virtual string GetSceneName(int seriesId, int seasonNumber = -1)
        {
            UpdateIfEmpty();

            var item = _database.FirstOrDefault<SceneMapping>("WHERE SeriesId = @0 AND SeasonNumber = @1", seriesId, seasonNumber);

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

        public virtual bool SubmitMapping(int id, string postTitle)
        {
            Logger.Trace("Parsing example post");
            var episodeParseResult = Parser.ParseTitle(postTitle);
            var cleanTitle = episodeParseResult.CleanTitle;
            var title = episodeParseResult.SeriesTitle.Replace('.', ' ');
            Logger.Trace("Example post parsed. CleanTitle: {0}, Title: {1}", cleanTitle, title);

            var newMapping = String.Format("/SceneMapping/AddPending?cleanTitle={0}&id={1}&title={2}", cleanTitle, id, title);
            var response = _httpProvider.DownloadString(_configProvider.ServiceRootUrl + newMapping);

            if (JsonConvert.DeserializeObject<String>(response).Equals("Ok"))
                return true;

            return false;
        }

        public virtual string GetCleanName(int seriesId)
        {
            var item = _database.FirstOrDefault<SceneMapping>("WHERE SeriesId = @0", seriesId);

            if (item == null)
                return null;

            return item.CleanTitle;
        }
    }
}
