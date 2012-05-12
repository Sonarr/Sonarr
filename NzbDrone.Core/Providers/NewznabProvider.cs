using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
            //Cleanup the URL if it is not null or whitespace
            if (!String.IsNullOrWhiteSpace(definition.Url))
                definition.Url = (new Uri(definition.Url).ParentUriString());

            if (definition.Id == 0)
            {
                Logger.Debug("Adding Newznab definitions for {0}", definition.Name);
                return Convert.ToInt32(_database.Insert(definition));
            }

            Logger.Debug("Updating Newznab definitions for {0}", definition.Name);
            return _database.Update(definition);
        }

        public virtual void SaveAll(IEnumerable<NewznabDefinition> definitions)
        {
            var definitionsList = definitions.ToList();

            //Cleanup the URL for each definition
            foreach(var newznabDefinition in definitionsList)
            {
                CheckHostname(newznabDefinition.Url);
                newznabDefinition.Url = new Uri(newznabDefinition.Url).ParentUriString();
            }

            _database.UpdateMany(definitionsList);
        }

        public virtual void InitializeNewznabIndexers(IList<NewznabDefinition> indexers)
        {
            Logger.Debug("Initializing Newznab indexers. Count {0}", indexers.Count);

            try
            {
                var currentIndexers = All();

                foreach(var feedProvider in indexers)
                {
                    try
                    {


                        NewznabDefinition indexerLocal = feedProvider;
                        var currentIndexer = currentIndexers
                                .FirstOrDefault(
                                                c =>
                                                new Uri(c.Url.ToLower()).Host == new Uri(indexerLocal.Url.ToLower()).Host);

                        if (currentIndexer == null)
                        {
                            var settings = new NewznabDefinition
                                               {
                                                   Enable = false,
                                                   Name = indexerLocal.Name,
                                                   Url = indexerLocal.Url,
                                                   ApiKey = indexerLocal.ApiKey,
                                                   BuiltIn = true
                                               };

                            Save(settings);
                        }

                        else
                        {
                            currentIndexer.BuiltIn = true;
                            Save(currentIndexer);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.ErrorException("An error occurred while setting up indexer: " + feedProvider.Name, ex);
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.ErrorException("An Error occurred while initializing Newznab Indexers", ex);
            }
        }

        public virtual void Delete(int id)
        {
            _database.Delete<NewznabDefinition>(id);
        }

        public virtual void CheckHostname(string url)
        {
            try
            {
                var uri = new Uri(url);
                var hostname = uri.DnsSafeHost;

                Dns.GetHostEntry(hostname);
            }
            catch (Exception ex)
            {
                Logger.Error("Invalid address {0}, please correct the site URL.", url);
                throw;
            }

        }
    }
}