using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(29)]
    public class add_formats_to_naming_config : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("NamingConfig").AddColumn("StandardEpisodeFormat").AsString().Nullable();
            Alter.Table("NamingConfig").AddColumn("DailyEpisodeFormat").AsString().Nullable();

            Execute.WithConnection(ConvertConfig);
        }

        private void ConvertConfig(IDbConnection conn, IDbTransaction tran)
        {
            using (IDbCommand namingConfigCmd = conn.CreateCommand())
            {
                namingConfigCmd.Transaction = tran;
                namingConfigCmd.CommandText = @"SELECT * FROM NamingConfig LIMIT 1";
                using (IDataReader namingConfigReader = namingConfigCmd.ExecuteReader())
                {
                    var separatorIndex = namingConfigReader.GetOrdinal("Separator");
                    var numberStyleIndex = namingConfigReader.GetOrdinal("NumberStyle");
                    var includeSeriesTitleIndex = namingConfigReader.GetOrdinal("IncludeSeriesTitle");
                    var includeEpisodeTitleIndex = namingConfigReader.GetOrdinal("IncludeEpisodeTitle");
                    var includeQualityIndex = namingConfigReader.GetOrdinal("IncludeQuality");
                    var replaceSpacesIndex = namingConfigReader.GetOrdinal("ReplaceSpaces");

                    while (namingConfigReader.Read())
                    {
                        var separator = namingConfigReader.GetString(separatorIndex);
                        var numberStyle = namingConfigReader.GetInt32(numberStyleIndex);
                        var includeSeriesTitle = namingConfigReader.GetBoolean(includeSeriesTitleIndex);
                        var includeEpisodeTitle = namingConfigReader.GetBoolean(includeEpisodeTitleIndex);
                        var includeQuality = namingConfigReader.GetBoolean(includeQualityIndex);
                        var replaceSpaces = namingConfigReader.GetBoolean(replaceSpacesIndex);

                        //Output settings
                        var seriesTitlePattern = "";
                        var episodeTitlePattern = "";
                        var dailyEpisodePattern = "{Air-Date}";
                        var qualityFormat = " [{Quality Title}]";

                        if (includeSeriesTitle)
                        {
                            if (replaceSpaces)
                            {
                                seriesTitlePattern = "{Series.Title}";
                            }
                            else
                            {
                                seriesTitlePattern = "{Series Title}";
                            }

                            seriesTitlePattern += separator;
                        }

                        if (includeEpisodeTitle)
                        {
                            episodeTitlePattern = separator;

                            if (replaceSpaces)
                            {
                                episodeTitlePattern += "{Episode.Title}";
                            }
                            else
                            {
                                episodeTitlePattern += "{Episode Title}";
                            }
                        }

                        var standardEpisodeFormat = string.Format("{0}{1}{2}",
                            seriesTitlePattern,
                            GetNumberStyle(numberStyle).Pattern,
                            episodeTitlePattern);

                        var dailyEpisodeFormat = string.Format("{0}{1}{2}",
                            seriesTitlePattern,
                            dailyEpisodePattern,
                            episodeTitlePattern);

                        if (includeQuality)
                        {
                            if (replaceSpaces)
                            {
                                qualityFormat = ".[{Quality.Title}]";
                            }

                            standardEpisodeFormat += qualityFormat;
                            dailyEpisodeFormat += qualityFormat;
                        }

                        using (IDbCommand updateCmd = conn.CreateCommand())
                        {
                            var text = string.Format("UPDATE NamingConfig " +
                                                     "SET StandardEpisodeFormat = '{0}', " +
                                                     "DailyEpisodeFormat = '{1}'",
                                                     standardEpisodeFormat,
                                                     dailyEpisodeFormat);

                            updateCmd.Transaction = tran;
                            updateCmd.CommandText = text;
                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        private static readonly List<dynamic> NumberStyles = new List<dynamic>
                                                                            {
                                                                                new
                                                                                    {
                                                                                        Id = 0,
                                                                                        Name = "1x05",
                                                                                        Pattern = "{season}x{episode:00}",
                                                                                        EpisodeSeparator = "x"
                                                                                    },
                                                                                new
                                                                                    {
                                                                                        Id = 1,
                                                                                        Name = "01x05",
                                                                                        Pattern = "{season:00}x{episode:00}",
                                                                                        EpisodeSeparator = "x"
                                                                                    },
                                                                                new
                                                                                    {
                                                                                        Id = 2,
                                                                                        Name = "S01E05",
                                                                                        Pattern = "S{season:00}E{episode:00}",
                                                                                        EpisodeSeparator = "E"
                                                                                    },
                                                                                new
                                                                                    {
                                                                                        Id = 3,
                                                                                        Name = "s01e05",
                                                                                        Pattern = "s{season:00}e{episode:00}",
                                                                                        EpisodeSeparator = "e"
                                                                                    }
                                                                            };

        private static dynamic GetNumberStyle(int id)
        {
            return NumberStyles.Single(s => s.Id == id);
        }
    }
}
