using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NLog;
using NzbDrone.Services.Service.Repository;
using Services.PetaPoco;

namespace NzbDrone.Services.Service.Providers
{
    public class SceneMappingProvider
    {
        private readonly IDatabase _database;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public SceneMappingProvider(IDatabase database)
        {
            _database = database;
        }

        public IList<SceneMapping> AllLive()
        {
            return _database.Fetch<SceneMapping>();
        }

        public IList<PendingSceneMapping> AllPending()
        {
            return _database.Fetch<PendingSceneMapping>();
        }

        public void Insert(SceneMapping sceneMapping)
        {
            _database.Insert(sceneMapping);
        }

        public void Insert(PendingSceneMapping pendingSceneMapping)
        {
            _database.Insert(pendingSceneMapping);
        }

        public void Update(PendingSceneMapping pendingSceneMapping)
        {
            _database.Update(pendingSceneMapping);
        }

        public PendingSceneMapping GetPending(int mappingId)
        {
            return _database.SingleOrDefault<PendingSceneMapping>("WHERE MappingId = @mappingId", new{ mappingId });
        }

        public void DeleteLive(int mappingId)
        {
            _database.Delete<SceneMapping>(mappingId);
        }

        public void DeletePending(int mappingId)
        {
            _database.Delete<PendingSceneMapping>(mappingId);
        }

        public bool Promote(int mappingId)
        {
            try
            {
                var pendingItem = GetPending(mappingId);

                var mapping = new SceneMapping
                {
                    CleanTitle = pendingItem.CleanTitle,
                    Id = pendingItem.Id,
                    Title = pendingItem.Title,
                    Season = -1
                };

                Insert(mapping);
                DeletePending(mappingId);
            }
            catch (Exception ex)
            {
                logger.WarnException("Unable to promote scene mapping", ex);
                return false;
            }

            return true;
        }

        public bool PromoteAll()
        {
            try
            {
                var pendingItems = _database.Fetch<PendingSceneMapping>();

                foreach (var pendingItem in pendingItems)
                {
                    Promote(pendingItem.MappingId);
                }
            }
            catch (Exception ex)
            {
                logger.WarnException("Unable to promote all scene mappings", ex);
                return false;
            }

            return true;
        }
    }
}