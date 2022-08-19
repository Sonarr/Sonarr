using System;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.EpisodeImport.Specifications;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.EpisodeImport.Specifications
{
    [TestFixture]
    public class AbsoluteEpisodeNumberSpecificationFixture : CoreTest<AbsoluteEpisodeNumberSpecification>
    {
        private Series _series;
        private LocalEpisode _localEpisode;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew()
                                     .With(s => s.SeriesType = SeriesTypes.Anime)
                                     .With(s => s.Path = @"C:\Test\TV\30 Rock".AsOsAgnostic())
                                     .Build();

            var episodes = Builder<Episode>.CreateListOfSize(1)
                                           .All()
                                           .With(e => e.SeasonNumber = 1)
                                           .With(e => e.AirDateUtc = DateTime.UtcNow)
                                           .Build()
                                           .ToList();

            _localEpisode = new LocalEpisode
                                {
                                    Path = @"C:\Test\Unsorted\30 Rock\30.rock.s01e01.avi".AsOsAgnostic(),
                                    Episodes = episodes,
                                    Series = _series
                                };

            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.RequiresAbsoluteEpisodeNumber())
                  .Returns(true);
        }

        [Test]
        public void should_reject_when_absolute_episode_number_is_null()
        {
            _localEpisode.Episodes.First().AbsoluteEpisodeNumber = null;

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_accept_when_did_not_air_recently_but_absolute_episode_number_is_null()
        {
            _localEpisode.Episodes.First().AirDateUtc = DateTime.UtcNow.AddDays(-7);
            _localEpisode.Episodes.First().AbsoluteEpisodeNumber = null;

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_when_absolute_episode_number_is_not_required()
        {
            _localEpisode.Episodes.First().AbsoluteEpisodeNumber = null;

            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.RequiresAbsoluteEpisodeNumber())
                  .Returns(false);

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }
    }
}
