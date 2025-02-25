using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators.Augmenters.Language;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MediaFiles.EpisodeImport.Aggregation.Aggregators.Augmenters.Language
{
    [TestFixture]
    public class AugmentLanguageFromMediaInfoFixture : CoreTest<AugmentLanguageFromMediaInfo>
    {
        [Test]
        public void should_return_null_if_media_info_is_null()
        {
            var localEpisode = Builder<LocalEpisode>.CreateNew()
                                                    .With(l => l.MediaInfo = null)
                                                    .Build();

            Subject.AugmentLanguage(localEpisode, null).Should().Be(null);
        }

        [Test]
        public void should_return_language_for_single_known_language()
        {
            var mediaInfo = Builder<MediaInfoModel>.CreateNew()
                                                   .With(m => m.AudioStreams = [
                                                       new MediaInfoAudioStreamModel { Language = "eng" },
                                                   ])
                                                   .Build();

            var localEpisode = Builder<LocalEpisode>.CreateNew()
                                                    .With(l => l.MediaInfo = mediaInfo)
                                                    .Build();

            var result = Subject.AugmentLanguage(localEpisode, null);

            result.Languages.Count.Should().Be(1);
            result.Languages.Should().Contain(Core.Languages.Language.English);
        }

        [Test]
        public void should_only_return_one_when_language_duplicated()
        {
            var mediaInfo = Builder<MediaInfoModel>.CreateNew()
                                                   .With(m => m.AudioStreams = [
                                                       new MediaInfoAudioStreamModel { Language = "eng" },
                                                       new MediaInfoAudioStreamModel { Language = "eng" },
                                                   ])
                                                   .Build();

            var localEpisode = Builder<LocalEpisode>.CreateNew()
                                                    .With(l => l.MediaInfo = mediaInfo)
                                                    .Build();

            var result = Subject.AugmentLanguage(localEpisode, null);

            result.Languages.Count.Should().Be(1);
            result.Languages.Should().Contain(Core.Languages.Language.English);
        }

        [Test]
        public void should_return_null_if_all_unknown()
        {
            var mediaInfo = Builder<MediaInfoModel>.CreateNew()
                                                   .With(m => m.AudioStreams = [
                                                       new MediaInfoAudioStreamModel { Language = "pirate" },
                                                       new MediaInfoAudioStreamModel { Language = "pirate" },
                                                   ])
                                                   .Build();

            var localEpisode = Builder<LocalEpisode>.CreateNew()
                                                    .With(l => l.MediaInfo = mediaInfo)
                                                    .Build();

            var result = Subject.AugmentLanguage(localEpisode, null);

            result.Should().BeNull();
        }

        [Test]
        public void should_return_known_languages_only()
        {
            var mediaInfo = Builder<MediaInfoModel>.CreateNew()
                                                   .With(m => m.AudioStreams = [
                                                       new MediaInfoAudioStreamModel { Language = "eng" },
                                                       new MediaInfoAudioStreamModel { Language = "pirate" },
                                                   ])
                                                   .Build();

            var localEpisode = Builder<LocalEpisode>.CreateNew()
                                                    .With(l => l.MediaInfo = mediaInfo)
                                                    .Build();

            var result = Subject.AugmentLanguage(localEpisode, null);

            result.Languages.Count.Should().Be(1);
            result.Languages.Should().Contain(Core.Languages.Language.English);
        }

        [Test]
        public void should_return_multiple_known_languages()
        {
            var mediaInfo = Builder<MediaInfoModel>.CreateNew()
                                                   .With(m => m.AudioStreams = [
                                                       new MediaInfoAudioStreamModel { Language = "eng" },
                                                       new MediaInfoAudioStreamModel { Language = "ger" },
                                                   ])
                                                   .Build();

            var localEpisode = Builder<LocalEpisode>.CreateNew()
                                                    .With(l => l.MediaInfo = mediaInfo)
                                                    .Build();

            var result = Subject.AugmentLanguage(localEpisode, null);

            result.Languages.Count.Should().Be(2);
            result.Languages.Should().Contain(Core.Languages.Language.English);
            result.Languages.Should().Contain(Core.Languages.Language.German);
        }
    }
}
