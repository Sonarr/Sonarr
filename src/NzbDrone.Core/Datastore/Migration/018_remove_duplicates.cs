using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;
using System.Linq;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(18)]
    public class remove_duplicates : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            using (var transaction = MigrationHelper.BeginTransaction())
            {
                RemoveDuplicateSeries<int>("TvdbId");
                RemoveDuplicateSeries<string>("TitleSlug");

                var duplicatedEpisodes = MigrationHelper.GetDuplicates<int>("Episodes", "TvDbEpisodeId");

                foreach (var duplicate in duplicatedEpisodes)
                {
                    foreach (var episodeId in duplicate.OrderBy(c => c.Key).Skip(1).Select(c => c.Key))
                    {
                        RemoveEpisodeRows(episodeId);
                    }
                }

                transaction.Commit();
            }
        }

        private void RemoveDuplicateSeries<T>(string field)
        {
            var duplicatedSeries = MigrationHelper.GetDuplicates<T>("Series", field);

            foreach (var duplicate in duplicatedSeries)
            {
                foreach (var seriesId in duplicate.OrderBy(c => c.Key).Skip(1).Select(c => c.Key))
                {
                    RemoveSeriesRows(seriesId);
                }
            }
        }

        private void RemoveSeriesRows(int seriesId)
        {
            MigrationHelper.ExecuteNonQuery("DELETE FROM Series WHERE Id = {0}", seriesId.ToString());
            MigrationHelper.ExecuteNonQuery("DELETE FROM Episodes WHERE SeriesId = {0}", seriesId.ToString());
            MigrationHelper.ExecuteNonQuery("DELETE FROM Seasons WHERE SeriesId = {0}", seriesId.ToString());
            MigrationHelper.ExecuteNonQuery("DELETE FROM History WHERE SeriesId = {0}", seriesId.ToString());
            MigrationHelper.ExecuteNonQuery("DELETE FROM EpisodeFiles WHERE SeriesId = {0}", seriesId.ToString());
        }

        private void RemoveEpisodeRows(int episodeId)
        {
            MigrationHelper.ExecuteNonQuery("DELETE FROM Episodes WHERE Id = {0}", episodeId.ToString());
            MigrationHelper.ExecuteNonQuery("DELETE FROM History WHERE EpisodeId = {0}", episodeId.ToString());
        }

    }
}
