using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(18)]
    public class remove_duplicates : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(RemoveDuplicates);
        }

        private void RemoveDuplicates(IDbConnection conn, IDbTransaction tran)
        {
            RemoveDuplicateSeries<int>(conn, tran, "TvdbId");
            RemoveDuplicateSeries<string>(conn, tran, "TitleSlug");

            var duplicatedEpisodes = GetDuplicates<int>(conn, tran, "Episodes", "TvDbEpisodeId");

            foreach (var duplicate in duplicatedEpisodes)
            {
                foreach (var episodeId in duplicate.OrderBy(c => c.Key).Skip(1).Select(c => c.Key))
                {
                    RemoveEpisodeRows(conn, tran, episodeId);
                }
            }
        }

        private IEnumerable<IGrouping<T, KeyValuePair<int, T>>> GetDuplicates<T>(IDbConnection conn, IDbTransaction tran, string tableName, string columnName)
        {
            var getDuplicates = conn.CreateCommand();
            getDuplicates.Transaction = tran;
            getDuplicates.CommandText = string.Format("select id, {0} from {1}", columnName, tableName);

            var result = new List<KeyValuePair<int, T>>();

            using (var reader = getDuplicates.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.Add(new KeyValuePair<int, T>(reader.GetInt32(0), (T)Convert.ChangeType(reader[1], typeof(T))));
                }
            }

            return result.GroupBy(c => c.Value).Where(g => g.Count() > 1);
        }

        private void RemoveDuplicateSeries<T>(IDbConnection conn, IDbTransaction tran, string field)
        {
            var duplicatedSeries = GetDuplicates<T>(conn, tran, "Series", field);

            foreach (var duplicate in duplicatedSeries)
            {
                foreach (var seriesId in duplicate.OrderBy(c => c.Key).Skip(1).Select(c => c.Key))
                {
                    RemoveSeriesRows(conn, tran, seriesId);
                }
            }
        }

        private void RemoveSeriesRows(IDbConnection conn, IDbTransaction tran, int seriesId)
        {
            var deleteCmd = conn.CreateCommand();
            deleteCmd.Transaction = tran;

            deleteCmd.CommandText = string.Format("DELETE FROM Series WHERE Id = {0}", seriesId.ToString());
            deleteCmd.ExecuteNonQuery();

            deleteCmd.CommandText = string.Format("DELETE FROM Episodes WHERE SeriesId = {0}", seriesId.ToString());
            deleteCmd.ExecuteNonQuery();

            deleteCmd.CommandText = string.Format("DELETE FROM Seasons WHERE SeriesId = {0}", seriesId.ToString());
            deleteCmd.ExecuteNonQuery();

            deleteCmd.CommandText = string.Format("DELETE FROM History WHERE SeriesId = {0}", seriesId.ToString());
            deleteCmd.ExecuteNonQuery();

            deleteCmd.CommandText = string.Format("DELETE FROM EpisodeFiles WHERE SeriesId = {0}", seriesId.ToString());
            deleteCmd.ExecuteNonQuery();
        }

        private void RemoveEpisodeRows(IDbConnection conn, IDbTransaction tran, int episodeId)
        {
            var deleteCmd = conn.CreateCommand();
            deleteCmd.Transaction = tran;

            deleteCmd.CommandText = string.Format("DELETE FROM Episodes WHERE Id = {0}", episodeId.ToString());
            deleteCmd.ExecuteNonQuery();

            deleteCmd.CommandText = string.Format("DELETE FROM History WHERE EpisodeId = {0}", episodeId.ToString());
            deleteCmd.ExecuteNonQuery();
        }
    }
}
