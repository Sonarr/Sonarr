using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.OrganizerTests.FileNameBuilderTests
{
    [TestFixture]
    public class CleanTitleFixture : CoreTest<FileNameBuilder>
    {
        private Series _series;
        private Episode _episode;
        private EpisodeFile _episodeFile;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>
                    .CreateNew()
                    .With(s => s.Title = "South Park")
                    .Build();

            _episode = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.SeasonNumber = 15)
                            .With(e => e.EpisodeNumber = 6)
                            .With(e => e.AbsoluteEpisodeNumber = 100)
                            .Build();

            _episodeFile = new EpisodeFile { Quality = new QualityModel(Quality.HDTV720p), ReleaseGroup = "SonarrTest" };

            _namingConfig = NamingConfig.Default;
            _namingConfig.RenameEpisodes = true;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(Moq.It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));
        }

        [TestCase("Florence + the Machine", "Florence + the Machine")]
        [TestCase("Beyoncé X10", "Beyoncé X10")]
        [TestCase("Girlfriends' Guide to Divorce", "Girlfriends Guide to Divorce")]
        [TestCase("Rule #23: Never Lie to the Kids", "Rule #23 Never Lie to the Kids")]
        [TestCase("Anne Hathaway/Florence + The Machine", "Anne Hathaway Florence + The Machine")]
        [TestCase("Chris Rock/Prince", "Chris Rock Prince")]
        [TestCase("Karma's a B*tch!", "Karmas a B-tch!")]
        [TestCase("Ke$ha: My Crazy Beautiful Life", "Ke$ha My Crazy Beautiful Life")]
        [TestCase("$#*! My Dad Says", "$#-! My Dad Says")]
        [TestCase("Free! - Iwatobi Swim Club", "Free! Iwatobi Swim Club")]
        [TestCase("Tamara Ecclestone: Billion $$ Girl", "Tamara Ecclestone Billion $$ Girl")]
        [TestCase("Marvel's Agents of S.H.I.E.L.D.", "Marvels Agents of S.H.I.E.L.D")]
        [TestCase("Castle (2009)", "Castle 2009")]
        [TestCase("Law & Order (UK)", "Law and Order UK")]
        [TestCase("Is this okay?", "Is this okay")]
        [TestCase("[a] title", "a title")]
        [TestCase("backslash \\ backlash", "backslash backlash")]
        [TestCase("I'm the Boss", "Im the Boss")]

        //[TestCase("", "")]
        public void should_get_expected_title_back(string title, string expected)
        {
            _series.Title = title;
            _namingConfig.StandardEpisodeFormat = "{Series CleanTitle}";

            Subject.BuildFileName(new List<Episode> { _episode }, _series, _episodeFile)
                   .Should().Be(expected);
        }

        [Test]
        public void should_use_and_as_separator_for_multiple_episodes()
        {
            var episodes = Builder<Episode>.CreateListOfSize(2)
                                           .TheFirst(1)
                                           .With(e => e.Title = "Surrender Benson")
                                           .TheNext(1)
                                           .With(e => e.Title = "Imprisoned Lives")
                                           .Build()
                                           .ToList();

            _namingConfig.StandardEpisodeFormat = "{Episode CleanTitle}";

            Subject.BuildFileName(episodes, _series, _episodeFile)
                   .Should().Be(episodes.First().Title + " and " + episodes.Last().Title);
        }
    }
}
