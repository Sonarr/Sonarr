using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Download.Aggregation.Aggregators;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Download.Aggregation.Aggregators
{
    [TestFixture]
    public class AggregateLanguagesFixture : CoreTest<AggregateLanguages>
    {
        private RemoteEpisode _remoteEpisode;
        private Series _series;
        private string _simpleReleaseTitle = "Series.Title.S01E01.xyz-RlsGroup";

        [SetUp]
        public void Setup()
        {
            var episodes = Builder<Episode>.CreateListOfSize(1)
                                           .BuildList();

            _series = Builder<Series>.CreateNew()
                       .With(m => m.OriginalLanguage = Language.English)
                       .Build();

            _remoteEpisode = Builder<RemoteEpisode>.CreateNew()
                                                 .With(l => l.ParsedEpisodeInfo = null)
                                                 .With(l => l.Episodes = episodes)
                                                 .With(l => l.Series = _series)
                                                 .Build();
        }

        private ParsedEpisodeInfo GetParsedEpisodeInfo(List<Language> languages, string releaseTitle, string releaseTokens = "")
        {
            return new ParsedEpisodeInfo
                   {
                       Languages = languages,
                       ReleaseTitle = releaseTitle,
                       ReleaseTokens = releaseTokens
                   };
        }

        [Test]
        public void should_return_existing_language_if_episode_title_does_not_have_language()
        {
            _remoteEpisode.ParsedEpisodeInfo = GetParsedEpisodeInfo(new List<Language> { Language.Original }, _simpleReleaseTitle);

            Subject.Aggregate(_remoteEpisode).Languages.Should().Contain(_series.OriginalLanguage);
        }

        [Test]
        public void should_return_parsed_language()
        {
            _remoteEpisode.ParsedEpisodeInfo = GetParsedEpisodeInfo(new List<Language> { Language.French }, _simpleReleaseTitle);

            Subject.Aggregate(_remoteEpisode).Languages.Should().Equal(_remoteEpisode.ParsedEpisodeInfo.Languages);
        }

        [Test]
        public void should_exclude_language_that_is_part_of_episode_title_when_release_tokens_contains_episode_title()
        {
            var releaseTitle = "Series.Title.S01E01.Jimmy.The.Greek.xyz-RlsGroup";
            var releaseTokens = ".Jimmy.The.Greek.xyz-RlsGroup";

            _remoteEpisode.Episodes.First().Title = "Jimmy The Greek";
            _remoteEpisode.ParsedEpisodeInfo = GetParsedEpisodeInfo(new List<Language> { Language.Greek }, releaseTitle, releaseTokens);

            Subject.Aggregate(_remoteEpisode).Languages.Should().Equal(_series.OriginalLanguage);
        }

        [Test]
        public void should_remove_parsed_language_that_is_part_of_episode_title_when_release_tokens_contains_episode_title()
        {
            var releaseTitle = "Series.Title.S01E01.Jimmy.The.Greek.French.xyz-RlsGroup";
            var releaseTokens = ".Jimmy.The.Greek.French.xyz-RlsGroup";

            _remoteEpisode.Episodes.First().Title = "Jimmy The Greek";
            _remoteEpisode.ParsedEpisodeInfo = GetParsedEpisodeInfo(new List<Language> { Language.Greek, Language.French }, releaseTitle, releaseTokens);

            Subject.Aggregate(_remoteEpisode).Languages.Should().Equal(Language.French);
        }

        [Test]
        public void should_not_exclude_language_that_is_part_of_episode_title_when_release_tokens_does_not_contain_episode_title()
        {
            var releaseTitle = "Series.Title.S01E01.xyz-RlsGroup";
            var releaseTokens = ".xyz-RlsGroup";

            _remoteEpisode.Episodes.First().Title = "Jimmy The Greek";
            _remoteEpisode.ParsedEpisodeInfo = GetParsedEpisodeInfo(new List<Language> { Language.Greek }, releaseTitle, releaseTokens);

            Subject.Aggregate(_remoteEpisode).Languages.Should().Equal(Language.Greek);
        }

        [Test]
        public void should_use_reparse_language_after_determining_languages_that_are_in_episode_titles()
        {
            var releaseTitle = "Series.Title.S01E01.Jimmy.The.Greek.Greek.xyz-RlsGroup";
            var releaseTokens = ".Jimmy.The.Greek.Greek.xyz-RlsGroup";

            _remoteEpisode.Episodes.First().Title = "Jimmy The Greek";
            _remoteEpisode.ParsedEpisodeInfo = GetParsedEpisodeInfo(new List<Language> { Language.Greek }, releaseTitle, releaseTokens);

            Subject.Aggregate(_remoteEpisode).Languages.Should().Equal(Language.Greek);
        }
    }
}
