using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Datastore.Converters;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class fix_pending_releasesFixture : MigrationTest<remove_plex_hometheatre>
    {
        [Test]
        public void should_fix_quality_for_pending_releases()
        {
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<ParsedEpisodeInfo162>());

            var db = WithDapperMigrationTestDb(c =>
            {
                c.Insert.IntoTable("PendingReleases").Row(new
                {
                    SeriesId = 1,
                    Title = "Test Series",
                    Added = DateTime.UtcNow,
                    ParsedEpisodeInfo = @"{
  ""releaseTitle"": ""Nurses.2020.S02E10.720p.HDTV.x264 - SYNCOPY"",
  ""seriesTitle"": ""Nurses 2020"",
  ""seriesTitleInfo"": {
    ""title"": ""Nurses 2020"",
    ""titleWithoutYear"": ""Nurses"",
    ""year"": 2020
  },
  ""quality"": {
    ""quality"": {
      ""id"": 4,
      ""name"": ""HDTV-720p"",
      ""source"": ""television"",
      ""resolution"": 720
    },
    ""revision"": {
      ""version"": 1,
      ""real"": 0,
      ""isRepack"": false
    }
  },
  ""seasonNumber"": 2,
  ""episodeNumbers"": [
    10
  ],
  ""absoluteEpisodeNumbers"": [],
  ""specialAbsoluteEpisodeNumbers"": [],
  ""language"": {
    ""id"": 1,
    ""name"": ""English""
  },
  ""fullSeason"": false,
  ""isPartialSeason"": false,
  ""isMultiSeason"": false,
  ""isSeasonExtra"": false,
  ""special"": false,
  ""releaseGroup"": ""SYNCOPY"",
  ""releaseHash"": """",
  ""seasonPart"": 0,
  ""releaseTokens"": "".720p.HDTV.x264-SYNCOPY"",
  ""isDaily"": false,
  ""isAbsoluteNumbering"": false,
  ""isPossibleSpecialEpisode"": false,
  ""isPossibleSceneSeasonSpecial"": false
}",
                    Release = "{}",
                    Reason = PendingReleaseReason.Delay
                });
            });

            var json = db.Query<string>("SELECT ParsedEpisodeInfo FROM PendingReleases").First();

            var pending = db.Query<ParsedEpisodeInfo162>("SELECT ParsedEpisodeInfo FROM PendingReleases").First();
            pending.Quality.Quality.Should().Be(Quality.HDTV720p.Id);
            pending.Language.Should().Be(Language.English.Id);
        }

        private class SeriesTitleInfo161
        {
            public string Title { get; set; }
            public string TitleWithoutYear { get; set; }
            public int Year { get; set; }
        }

        private class ParsedEpisodeInfo162
        {
            public string SeriesTitle { get; set; }
            public SeriesTitleInfo161 SeriesTitleInfo { get; set; }
            public QualityModel162 Quality { get; set; }
            public int SeasonNumber { get; set; }
            public List<int> EpisodeNumbers { get; set; }
            public List<int> AbsoluteEpisodeNumbers { get; set; }
            public List<int> SpecialAbsoluteEpisodeNumbers { get; set; }
            public int Language { get; set; }
            public bool FullSeason { get; set; }
            public bool IsPartialSeason { get; set; }
            public bool IsMultiSeason { get; set; }
            public bool IsSeasonExtra { get; set; }
            public bool Speacial { get; set; }
            public string ReleaseGroup { get; set; }
            public string ReleaseHash { get; set; }
            public int SeasonPart { get; set; }
            public string ReleaseTokens { get; set; }
            public bool IsDaily { get; set; }
            public bool IsAbsoluteNumbering { get; set; }
            public bool IsPossibleSpecialEpisode { get; set; }
            public bool IsPossibleSceneSeasonSpecial { get; set; }
        }

        private class QualityModel162
        {
            public int Quality { get; set; }
            public Revision162 Revision { get; set; }
        }

        private class Revision162
        {
            public int Version { get; set; }
            public int Real { get; set; }
            public bool IsRepack { get; set; }
        }
    }
}
