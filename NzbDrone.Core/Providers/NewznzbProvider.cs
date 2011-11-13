using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using NLog;
using NzbDrone.Core.Providers.Indexer;
using NzbDrone.Core.Repository;
using PetaPoco;

namespace NzbDrone.Core.Providers
{
    public class NewznabProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IDatabase _database;

        [Inject]
        public NewznabProvider(IDatabase database)
        {
            _database = database;
        }

        public NewznabProvider()
        {

        }

        public virtual List<NewznabDefinition> Enabled()
        {
            return _database.Fetch<NewznabDefinition>("WHERE Enable = 1");
        }

        public virtual List<NewznabDefinition> All()
        {
            return _database.Fetch<NewznabDefinition>();
        }

        public virtual int Save(NewznabDefinition definition)
        {
            //Cleanup the URL
            definition.Url = (new Uri(definition.Url).ParentUriString());

            if (definition.Id == 0)
            {
                Logger.Debug("Adding Newznab definitions for {0}", definition.Name);
                return Convert.ToInt32(_database.Insert(definition));
            }

            else
            {
                Logger.Debug("Updating Newznab definitions for {0}", definition.Name);
                return _database.Update(definition);
            }
        }

        public virtual void SaveAll(IEnumerable<NewznabDefinition> definitions)
        {
            var definitionsList = definitions.ToList();

            //Cleanup the URL for each definition
            definitionsList.ForEach(p => p.Url = (new Uri(p.Url).ParentUriString()));

            _database.UpdateMany(definitionsList);
        }

        public virtual void InitializeNewznabIndexers(IList<NewznabDefinition> indexers)
        {
            Logger.Info("Initializing Newznab indexers. Count {0}", indexers.Count);

            var currentIndexers = All();

            foreach (var feedProvider in indexers)
            {
                NewznabDefinition indexerLocal = feedProvider;
                if (!currentIndexers.Exists(c => c.Name == indexerLocal.Name))
                {
                    var settings = new NewznabDefinition
                                       {
                                           Enable = false,
                                           Name = indexerLocal.Name,
                                           Url = indexerLocal.Url,
                                           ApiKey = indexerLocal.ApiKey
                                       };

                    Save(settings);
                }
            }
        }

        public virtual void Delete(int id)
        {
            _database.Delete<NewznabDefinition>(id);
        }
    }
}