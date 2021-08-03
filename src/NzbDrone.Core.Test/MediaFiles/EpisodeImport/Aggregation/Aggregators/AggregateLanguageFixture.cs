using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.MediaFiles.EpisodeImport.Aggregation.Aggregators
{
    [TestFixture]
    public class AggregateLanguageFixture : CoreTest<AggregateLanguage>
    {
        private LocalEpisode _localEpisode;
        private string _simpleReleaseTitle = "Series.Title.S01E01.xyz-RlsGroup";

        [SetUp]
        public void Setup()
        {
            var episodes = Builder<Episode>.CreateListOfSize(1)
                                           .BuildList();

            _localEpisode = Builder<LocalEpisode>.CreateNew()
                                                 .With(l => l.DownloadClientEpisodeInfo = null)
                                                 .With(l => l.FolderEpisodeInfo = null)
                                                 .With(l => l.FileEpisodeInfo = null)
                                                 .With(l => l.Episodes = episodes)
                                                 .Build();
        }

        private ParsedEpisodeInfo GetParsedEpisodeInfo(Language language, string releaseTitle)
        {
            return new ParsedEpisodeInfo
                   {
                       Language = language,
                       ReleaseTitle = releaseTitle
                   };
        }

        [Test]
        public void should_return_file_language_when_only_file_info_is_known()
        {
            _localEpisode.FileEpisodeInfo = GetParsedEpisodeInfo(Language.English, _simpleReleaseTitle);

            Subject.Aggregate(_localEpisode, null).Language.Should().Be(_localEpisode.FileEpisodeInfo.Language);
        }

        [Test]
        public void should_return_folder_language_when_folder_info_is_known()
        {
            _localEpisode.FolderEpisodeInfo = GetParsedEpisodeInfo(Language.English, _simpleReleaseTitle);
            _localEpisode.FileEpisodeInfo = GetParsedEpisodeInfo(Language.English, _simpleReleaseTitle);

            Subject.Aggregate(_localEpisode, null).Language.Should().Be(_localEpisode.FolderEpisodeInfo.Language);
        }

        [Test]
        public void should_return_download_client_item_language_when_download_client_item_info_is_known()
        {
            _localEpisode.DownloadClientEpisodeInfo = GetParsedEpisodeInfo(Language.English, _simpleReleaseTitle);
            _localEpisode.FolderEpisodeInfo = GetParsedEpisodeInfo(Language.English, _simpleReleaseTitle);
            _localEpisode.FileEpisodeInfo = GetParsedEpisodeInfo(Language.English, _simpleReleaseTitle);

            Subject.Aggregate(_localEpisode, null).Language.Should().Be(_localEpisode.DownloadClientEpisodeInfo.Language);
        }

        [Test]
        public void should_return_file_language_when_file_language_is_higher_than_others()
        {
            _localEpisode.DownloadClientEpisodeInfo = GetParsedEpisodeInfo(Language.English, _simpleReleaseTitle);
            _localEpisode.FolderEpisodeInfo = GetParsedEpisodeInfo(Language.English, _simpleReleaseTitle);
            _localEpisode.FileEpisodeInfo = GetParsedEpisodeInfo(Language.French, _simpleReleaseTitle);

            Subject.Aggregate(_localEpisode, null).Language.Should().Be(_localEpisode.FileEpisodeInfo.Language);
        }

        [Test]
        public void should_return_english_if_parsed_language_is_in_episode_title_and_release_title_contains_episode_title()
        {
            _localEpisode.Episodes.First().Title = "The Swedish Job";
            _localEpisode.FileEpisodeInfo = GetParsedEpisodeInfo(Language.Swedish, "Series.Title.S01E01.The.Swedish.Job.720p.WEB-DL-RlsGrp");

            Subject.Aggregate(_localEpisode, null).Language.Should().Be(Language.English);
        }

        [Test]
        public void should_return_parsed_if_parsed_language_is_not_episode_title_and_release_title_contains_episode_title()
        {
            _localEpisode.Episodes.First().Title = "The Swedish Job";
            _localEpisode.FileEpisodeInfo = GetParsedEpisodeInfo(Language.French, "Series.Title.S01E01.The.Swedish.Job.720p.WEB-DL-RlsGrp");

            Subject.Aggregate(_localEpisode, null).Language.Should().Be(_localEpisode.FileEpisodeInfo.Language);
        }
    }
}
