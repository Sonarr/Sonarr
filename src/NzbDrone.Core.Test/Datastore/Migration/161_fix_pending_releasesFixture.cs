using System;
using System.Linq;
using Dapper;
using FluentAssertions;
using NUnit.Framework;
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

            var pending = db.Query<ParsedEpisodeInfo>("SELECT ParsedEpisodeInfo FROM PendingReleases").First();
            pending.Quality.Quality.Should().Be(Quality.HDTV720p);
            pending.Language.Should().Be(Language.English);
        }
    }
}
