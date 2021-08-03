using System.Collections.Generic;
using System.IO;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Languages;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.EpisodeImport
{
    [TestFixture]
    public class GetSceneNameFixture : CoreTest
    {
        private LocalEpisode _localEpisode;
        private string _seasonName = "series.title.s02.dvdrip.x264-ingot";
        private string _episodeName = "series.title.s02e23.dvdrip.x264-ingot";

        [SetUp]
        public void Setup()
        {
            var series = Builder<Series>.CreateNew()
                                        .With(e => e.QualityProfile = new QualityProfile { Items = Qualities.QualityFixture.GetDefaultQualities() })
                                        .With(l => l.LanguageProfile = new LanguageProfile
                                                                       {
                                                                           Cutoff = Language.Spanish,
                                                                           Languages = Languages.LanguageFixture.GetDefaultLanguages()
                                                                       })
                                        .With(s => s.Path = @"C:\Test\TV\Series Title".AsOsAgnostic())
                                        .Build();

            var episode = Builder<Episode>.CreateNew()
                                          .Build();

            _localEpisode = new LocalEpisode
                            {
                                Series = series,
                                Episodes = new List<Episode> { episode },
                                Path = Path.Combine(series.Path, "Series Title - S02E23 - Episode Title.mkv"),
                                Quality = new QualityModel(Quality.Bluray720p),
                                ReleaseGroup = "DRONE"
                            };
        }

        private void GivenExistingFileOnDisk()
        {
            Mocker.GetMock<IMediaFileService>()
                  .Setup(s => s.GetFilesWithRelativePath(It.IsAny<int>(), It.IsAny<string>()))
                  .Returns(new List<EpisodeFile>());
        }

        [Test]
        public void should_use_download_client_item_title_as_scene_name()
        {
            _localEpisode.DownloadClientEpisodeInfo = new ParsedEpisodeInfo
                                                      {
                                                          ReleaseTitle = _episodeName
                                                      };

            SceneNameCalculator.GetSceneName(_localEpisode).Should()
                               .Be(_episodeName);
        }

        [Test]
        public void should_not_use_download_client_item_title_as_scene_name_if_full_season()
        {
            _localEpisode.DownloadClientEpisodeInfo = new ParsedEpisodeInfo
                                                      {
                                                          ReleaseTitle = _seasonName,
                                                          FullSeason = true
                                                      };

            _localEpisode.Path = Path.Combine(@"C:\Test\Unsorted TV", _seasonName, _episodeName)
                                     .AsOsAgnostic();

            SceneNameCalculator.GetSceneName(_localEpisode).Should()
                               .BeNull();
        }

        [Test]
        public void should_not_use_download_client_item_title_as_scene_name_if_there_are_other_video_files()
        {
            _localEpisode.OtherVideoFiles = true;
            _localEpisode.DownloadClientEpisodeInfo = new ParsedEpisodeInfo
                                                      {
                                                          ReleaseTitle = _seasonName,
                                                          FullSeason = false
                                                      };

            _localEpisode.Path = Path.Combine(@"C:\Test\Unsorted TV", _seasonName, _episodeName)
                                     .AsOsAgnostic();

            SceneNameCalculator.GetSceneName(_localEpisode).Should()
                               .BeNull();
        }

        [Test]
        public void should_use_file_name_as_scenename_only_if_it_looks_like_scenename()
        {
            _localEpisode.Path = Path.Combine(@"C:\Test\Unsorted TV", _seasonName, _episodeName + ".mkv")
                                     .AsOsAgnostic();

            SceneNameCalculator.GetSceneName(_localEpisode).Should()
                               .Be(_episodeName);
        }

        [Test]
        public void should_not_use_file_name_as_scenename_if_it_doesnt_look_like_scenename()
        {
            _localEpisode.Path = Path.Combine(@"C:\Test\Unsorted TV", _episodeName, "aaaaa.mkv")
                                     .AsOsAgnostic();

            SceneNameCalculator.GetSceneName(_localEpisode).Should()
                               .BeNull();
        }

        [Test]
        public void should_use_folder_name_as_scenename_only_if_it_looks_like_scenename()
        {
            _localEpisode.FolderEpisodeInfo = new ParsedEpisodeInfo
                                              {
                                                  ReleaseTitle = _episodeName
                                              };

            SceneNameCalculator.GetSceneName(_localEpisode).Should()
                               .Be(_episodeName);
        }

        [Test]
        public void should_not_use_folder_name_as_scenename_if_it_doesnt_look_like_scenename()
        {
            _localEpisode.Path = Path.Combine(@"C:\Test\Unsorted TV", _episodeName, "aaaaa.mkv")
                                     .AsOsAgnostic();

            _localEpisode.FolderEpisodeInfo = new ParsedEpisodeInfo
                                              {
                                                  ReleaseTitle = "aaaaa"
                                              };

            SceneNameCalculator.GetSceneName(_localEpisode).Should()
                               .BeNull();
        }

        [Test]
        public void should_not_use_folder_name_as_scenename_if_it_is_for_a_full_season()
        {
            _localEpisode.Path = Path.Combine(@"C:\Test\Unsorted TV", _episodeName, "aaaaa.mkv")
                                     .AsOsAgnostic();

            _localEpisode.FolderEpisodeInfo = new ParsedEpisodeInfo
                                              {
                                                  ReleaseTitle = _seasonName,
                                                  FullSeason = true
                                              };

            SceneNameCalculator.GetSceneName(_localEpisode).Should()
                               .BeNull();
        }

        [Test]
        public void should_not_use_folder_name_as_scenename_if_it_is_for_batch()
        {
            var batchName = "[HorribleSubs] Series Title (01-62) [1080p] (Batch)";

            _localEpisode.DownloadClientEpisodeInfo = new ParsedEpisodeInfo
                                                      {
                                                          FullSeason = false,
                                                          ReleaseTitle = batchName
                                                      };

            _localEpisode.Path = Path.Combine(@"C:\Test\Unsorted TV", batchName, "[HorribleSubs] Series Title - 14 [1080p].mkv")
                                     .AsOsAgnostic();

            _localEpisode.OtherVideoFiles = true;

            _localEpisode.FolderEpisodeInfo = new ParsedEpisodeInfo
                                              {
                                                  ReleaseTitle = _seasonName,
                                                  FullSeason = false
                                              };

            SceneNameCalculator.GetSceneName(_localEpisode).Should()
                               .BeNull();
        }

        [Test]
        public void should_not_use_folder_name_as_scenename_if_there_are_other_video_files()
        {
            _localEpisode.OtherVideoFiles = true;
            _localEpisode.Path = Path.Combine(@"C:\Test\Unsorted TV", _episodeName, "aaaaa.mkv")
                                     .AsOsAgnostic();

            _localEpisode.FolderEpisodeInfo = new ParsedEpisodeInfo
                                              {
                                                  ReleaseTitle = _seasonName,
                                                  FullSeason = false
                                              };

            SceneNameCalculator.GetSceneName(_localEpisode).Should()
                               .BeNull();
        }

        [TestCase(".mkv")]
        [TestCase(".par2")]
        [TestCase(".nzb")]
        public void should_remove_extension_from_nzb_title_for_scene_name(string extension)
        {
            _localEpisode.DownloadClientEpisodeInfo = new ParsedEpisodeInfo
                                                      {
                                                          ReleaseTitle = _episodeName + extension
                                                      };

            SceneNameCalculator.GetSceneName(_localEpisode).Should()
                               .Be(_episodeName);
        }
    }
}
