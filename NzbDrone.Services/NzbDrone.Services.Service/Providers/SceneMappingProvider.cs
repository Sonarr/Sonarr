using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NzbDrone.Services.Service.Repository;
using PetaPoco;

namespace NzbDrone.Services.Service.Providers
{
    public class SceneMappingProvider
    {
        private readonly IDatabase _database;

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

        public void DeleteLive(string cleanTitle)
        {
            _database.Delete<SceneMapping>(cleanTitle);
        }

        public void DeletePending(string cleanTitle)
        {
            _database.Delete<PendingSceneMapping>(cleanTitle);
        }

        public bool Promote(string cleanTitle)
        {
            try
            {
                var pendingItem = _database.Single<PendingSceneMapping>(cleanTitle);

                var mapping = new SceneMapping
                {
                    CleanTitle = pendingItem.CleanTitle,
                    Id = pendingItem.Id,
                    Title = pendingItem.Title
                };

                _database.Insert(mapping);
                _database.Delete(pendingItem);
            }
            catch (Exception ex)
            {
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
                    Promote(pendingItem.Title);
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }
    }
}