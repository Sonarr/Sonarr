using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using FluentMigrator;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(79)]
    public class dedupe_tags : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(CleanupTags);

            Alter.Table("Tags").AlterColumn("Label").AsString().Unique();
        }

        private void CleanupTags(IDbConnection conn, IDbTransaction tran)
        {
            var tags = GetTags(conn, tran);
            var grouped = tags.GroupBy(t => t.Label.ToLowerInvariant());
            var replacements = new List<TagReplacement079>();

            foreach (var group in grouped.Where(g => g.Count() > 1))
            {
                var first = group.First().Id;

                foreach (var other in group.Skip(1).Select(t => t.Id))
                {
                    replacements.Add(new TagReplacement079 { OldId = other, NewId = first });
                }
            }

            UpdateTaggedModel(conn, tran, "Series", replacements);
            UpdateTaggedModel(conn, tran, "Notifications", replacements);
            UpdateTaggedModel(conn, tran, "DelayProfiles", replacements);
            UpdateTaggedModel(conn, tran, "Restrictions", replacements);

            DeleteTags(conn, tran, replacements);
        }

        private List<Tag079> GetTags(IDbConnection conn, IDbTransaction tran)
        {
            var tags = new List<Tag079>();

            using (var tagCmd = conn.CreateCommand())
            {
                tagCmd.Transaction = tran;
                tagCmd.CommandText = "SELECT \"Id\", \"Label\" FROM \"Tags\"";

                using (var tagReader = tagCmd.ExecuteReader())
                {
                    while (tagReader.Read())
                    {
                        var id = tagReader.GetInt32(0);
                        var label = tagReader.GetString(1);

                        tags.Add(new Tag079 { Id = id, Label = label });
                    }
                }
            }

            return tags;
        }

        private void UpdateTaggedModel(IDbConnection conn, IDbTransaction tran, string table, List<TagReplacement079> replacements)
        {
            var tagged = new List<TaggedModel079>();

            using (var tagCmd = conn.CreateCommand())
            {
                tagCmd.Transaction = tran;
                tagCmd.CommandText = string.Format("SELECT \"Id\", \"Tags\" FROM \"{0}\"", table);

                using (var tagReader = tagCmd.ExecuteReader())
                {
                    while (tagReader.Read())
                    {
                        if (!tagReader.IsDBNull(1))
                        {
                            var id = tagReader.GetInt32(0);
                            var tags = tagReader.GetString(1);

                            tagged.Add(new TaggedModel079
                                       {
                                           Id = id,
                                           Tags = Json.Deserialize<HashSet<int>>(tags)
                                       });
                        }
                    }
                }
            }

            var toUpdate = new List<TaggedModel079>();

            foreach (var model in tagged)
            {
                foreach (var replacement in replacements)
                {
                    if (model.Tags.Contains(replacement.OldId))
                    {
                        model.Tags.Remove(replacement.OldId);
                        model.Tags.Add(replacement.NewId);

                        toUpdate.Add(model);
                    }
                }
            }

            var updatedTags = toUpdate.DistinctBy(m => m.Id).Select(t => new
            {
                Tags = t.Tags.ToJson(),
                Id = t.Id
            });

            var updateTagsSql = $"UPDATE \"{table}\" SET \"Tags\" = @Tags WHERE \"Id\" = @Id";
            conn.Execute(updateTagsSql, updatedTags, transaction: tran);
        }

        private void DeleteTags(IDbConnection conn, IDbTransaction tran, List<TagReplacement079> replacements)
        {
            var idsToRemove = replacements.Select(r => r.OldId).Distinct();

            if (idsToRemove.Any())
            {
                using (var removeCmd = conn.CreateCommand())
                {
                    removeCmd.Transaction = tran;
                    removeCmd.CommandText = $"DELETE FROM \"Tags\" WHERE \"Id\" IN ({string.Join(", ", idsToRemove)})";
                    removeCmd.ExecuteNonQuery();
                }
            }
        }

        private class Tag079
        {
            public int Id { get; set; }
            public string Label { get; set; }
        }

        private class TagReplacement079
        {
            public int OldId { get; set; }
            public int NewId { get; set; }
        }

        private class TaggedModel079
        {
            public int Id { get; set; }
            public HashSet<int> Tags { get; set; }
        }
    }
}
