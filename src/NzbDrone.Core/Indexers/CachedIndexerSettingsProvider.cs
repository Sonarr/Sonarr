using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider.Events;

namespace NzbDrone.Core.Indexers;

public interface ICachedIndexerSettingsProvider
{
    CachedIndexerSettings GetSettings(int indexerId);
}

public class CachedIndexerSettingsProvider : ICachedIndexerSettingsProvider, IHandle<ProviderUpdatedEvent<IIndexer>>, IHandle<ProviderDeletedEvent<IIndexer>>
{
    private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(CachedIndexerSettingsProvider));

    private readonly IIndexerFactory _indexerFactory;
    private readonly ICached<CachedIndexerSettings> _cache;

    public CachedIndexerSettingsProvider(IIndexerFactory indexerFactory, ICacheManager cacheManager)
    {
        _indexerFactory = indexerFactory;
        _cache = cacheManager.GetRollingCache<CachedIndexerSettings>(GetType(), "settingsByIndexer", TimeSpan.FromHours(1));
    }

    public CachedIndexerSettings GetSettings(int indexerId)
    {
        if (indexerId == 0)
        {
            return null;
        }

        return _cache.Get(indexerId.ToString(), () => FetchIndexerSettings(indexerId));
    }

    private CachedIndexerSettings FetchIndexerSettings(int indexerId)
    {
        var indexer = _indexerFactory.Find(indexerId);

        if (indexer?.Settings is not IIndexerSettings indexerSettings)
        {
            Logger.Trace("Could not load settings for indexer ID: {0}", indexerId);

            return null;
        }

        var settings = new CachedIndexerSettings
        {
            FailDownloads = indexerSettings.FailDownloads.Select(f => (FailDownloads)f).ToHashSet()
        };

        if (indexer.Settings is ITorrentIndexerSettings torrentIndexerSettings)
        {
            settings.SeedCriteriaSettings = torrentIndexerSettings.SeedCriteria;
        }

        return settings;
    }

    public void Handle(ProviderUpdatedEvent<IIndexer> message)
    {
        _cache.Clear();
    }

    public void Handle(ProviderDeletedEvent<IIndexer> message)
    {
        _cache.Clear();
    }
}

public class CachedIndexerSettings
{
    public HashSet<FailDownloads> FailDownloads { get; set; }
    public SeedCriteriaSettings SeedCriteriaSettings { get; set; }
}
