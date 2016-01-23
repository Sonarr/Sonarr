using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.History;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Statistics
{
    public interface IStatisticsService
    {
        StatisticsGrouping<HistoryStatistics> GetGlobalStatistics();
        Dictionary<string, StatisticsGrouping<HistoryStatistics>> GetIndexerStatistics();
    }

    public class StatisticsService : IStatisticsService
    {
        private readonly IHistoryRepository _historyRepository;

        public StatisticsService(IHistoryRepository historyRepository)
        {
            _historyRepository = historyRepository;
        }

        public StatisticsGrouping<HistoryStatistics> GetGlobalStatistics()
        {
            var stats = new StatisticsGrouping<HistoryStatistics>();

            foreach (var group in GetIndexerStatistics().Values)
            {
                stats.LastWeek.Grabs += group.LastWeek.Grabs;
                stats.LastWeek.Replaced += group.LastWeek.Replaced;
                stats.LastWeek.Failed += group.LastWeek.Failed;
                stats.LastWeek.Imported += group.LastWeek.Imported;

                stats.LastMonth.Grabs += group.LastMonth.Grabs;
                stats.LastMonth.Replaced += group.LastMonth.Replaced;
                stats.LastMonth.Failed += group.LastMonth.Failed;
                stats.LastMonth.Imported += group.LastMonth.Imported;

                stats.AllTime.Grabs += group.AllTime.Grabs;
                stats.AllTime.Replaced += group.AllTime.Replaced;
                stats.AllTime.Failed += group.AllTime.Failed;
                stats.AllTime.Imported += group.AllTime.Imported;
            }

            return stats;
        }

        public Dictionary<string, StatisticsGrouping<HistoryStatistics>> GetIndexerStatistics()
        {
            var all = _historyRepository.All().ToArray();

            var stats = new Dictionary<string, StatisticsGrouping<HistoryStatistics>>();
            stats[string.Empty] = new StatisticsGrouping<HistoryStatistics>();

            var groupedByEpisode = all.GroupBy(v => v.EpisodeId).ToArray();

            foreach (var episode in groupedByEpisode)
            {
                var sortedEvents = episode.OrderBy(v => v.DownloadId)
                                          .ThenBy(v => v.Date)
                                          .ThenBy(v => v.Id)
                                          .ToArray();

                var lastEvent = HistoryEventType.Unknown;
                string grabIndexer = null;
                string importIndexer = null;

                foreach (var historyEvent in sortedEvents)
                {
                    switch (historyEvent.EventType)
                    {
                        // Episode got grabbed from a specific indexer. Attribute anything that happens to that indexer.
                        case History.HistoryEventType.Grabbed:
                            grabIndexer = historyEvent.Data.GetValueOrDefault("indexer") ?? string.Empty;
                            Apply(stats, grabIndexer, historyEvent.Date, s => s.Grabs++);
                            lastEvent = HistoryEventType.Grabbed;
                            break;

                        // Episodes got imported, only attribute the import if we grabbed it from an indexer.
                        // Try attribute the deletion/replacement to the previous indexer.
                        case History.HistoryEventType.SeriesFolderImported:
                        case History.HistoryEventType.DownloadFolderImported:
                            if (lastEvent == HistoryEventType.Grabbed)
                            {
                                if (importIndexer != null)
                                {
                                    Apply(stats, importIndexer, historyEvent.Date, s => s.Replaced++);
                                }
                                importIndexer = grabIndexer;
                                grabIndexer = null;
                                Apply(stats, importIndexer, historyEvent.Date, s => s.Imported++);
                                lastEvent = HistoryEventType.DownloadFolderImported;
                            }
                            else
                            {
                                lastEvent = HistoryEventType.Unknown;
                            }
                            break;

                        // Attribute the failure to the indexer if we haven't imported yet.
                        case History.HistoryEventType.DownloadFailed:
                            if (lastEvent == HistoryEventType.Grabbed)
                            {
                                Apply(stats, grabIndexer, historyEvent.Date, s => s.Failed++);
                                grabIndexer = null;
                                lastEvent = HistoryEventType.Unknown;
                            }
                            break;

                        case History.HistoryEventType.EpisodeFileDeleted:
                            lastEvent = HistoryEventType.Unknown;
                            break;

                        case History.HistoryEventType.Unknown:
                        default:
                            break;
                    }
                }
            }

            return stats;
        }

        private void Apply<T>(Dictionary<string, StatisticsGrouping<T>> stats, string key, DateTime date, Action<T> applyAction) where T : new()
        {
            StatisticsGrouping<T> group;

            if (!stats.TryGetValue(key, out group))
            {
                stats[key] = group = new StatisticsGrouping<T>();
            }

            group.Apply(date, applyAction);
        }
    }
}