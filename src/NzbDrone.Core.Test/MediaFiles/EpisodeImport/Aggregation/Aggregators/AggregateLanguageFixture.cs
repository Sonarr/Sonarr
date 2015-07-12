using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MediaFiles.EpisodeImport.Aggregation.Aggregators
{
    [TestFixture]  
    public class AggregateLanguageFixture : CoreTest<AggregateLanguage>
    {
        private LocalEpisode _localEpisode;

        [SetUp]
        public void Setup()
        {
            _localEpisode = Builder<LocalEpisode>.CreateNew()
                                                 .With(l => l.DownloadClientEpisodeInfo = null)
                                                 .With(l => l.FolderEpisodeInfo = null)
                                                 .With(l => l.FileEpisodeInfo = null)
                                                 .Build();
        }

        private ParsedEpisodeInfo GetParsedEpisodeInfo(Language language)
        {
            return new ParsedEpisodeInfo
                   {
                       Language = language
                   };
        }

        [Test]
        public void should_return_file_language_when_only_file_info_is_known()
        {
            _localEpisode.FileEpisodeInfo = GetParsedEpisodeInfo(Language.English);

            Subject.Aggregate(_localEpisode, false).Language.Should().Be(_localEpisode.FileEpisodeInfo.Language);
        }

        [Test]
        public void should_return_folder_language_when_folder_info_is_known()
        {
            _localEpisode.FolderEpisodeInfo = GetParsedEpisodeInfo(Language.English);
            _localEpisode.FileEpisodeInfo = GetParsedEpisodeInfo(Language.English);

            Subject.Aggregate(_localEpisode, false).Language.Should().Be(_localEpisode.FolderEpisodeInfo.Language);
        }

        [Test]
        public void should_return_download_client_item_language_when_download_client_item_info_is_known()
        {
            _localEpisode.DownloadClientEpisodeInfo = GetParsedEpisodeInfo(Language.English);
            _localEpisode.FolderEpisodeInfo = GetParsedEpisodeInfo(Language.English);
            _localEpisode.FileEpisodeInfo = GetParsedEpisodeInfo(Language.English);

            Subject.Aggregate(_localEpisode, false).Language.Should().Be(_localEpisode.DownloadClientEpisodeInfo.Language);

        }

        [Test]
        public void should_return_file_language_when_file_language_is_higher_than_others()
        {
            _localEpisode.DownloadClientEpisodeInfo = GetParsedEpisodeInfo(Language.English);
            _localEpisode.FolderEpisodeInfo = GetParsedEpisodeInfo(Language.English);
            _localEpisode.FileEpisodeInfo = GetParsedEpisodeInfo(Language.French);

            Subject.Aggregate(_localEpisode, false).Language.Should().Be(_localEpisode.FileEpisodeInfo.Language);
        }
    }
}
