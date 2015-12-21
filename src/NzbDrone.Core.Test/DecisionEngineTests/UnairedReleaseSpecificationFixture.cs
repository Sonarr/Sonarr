
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class UnairedReleaseSpecificationFixture : CoreTest<UnairedReleaseSpecification>
    {
        private RemoteEpisode _goodRemoteEpisode;
        private RemoteEpisode _fakeRemoteEpisode;

        [SetUp]
        public void Setup()
        {
            var show = Builder<Series>.CreateNew().With(s => s.Id = 1234).Build();

            _goodRemoteEpisode = new RemoteEpisode
            {
                Episodes = Builder<Episode>.CreateListOfSize(2)
                                           .All()
                                           .With(e => e.AirDateUtc = System.DateTime.UtcNow.AddDays(-8))
                                           .With(s => s.SeriesId = show.Id)
                                           .BuildList(),
                Series = show,
                Release = new ReleaseInfo
                {
                    PublishDate = System.DateTime.Now
                }
            };

            _fakeRemoteEpisode = new RemoteEpisode
            {
                Episodes = Builder<Episode>.CreateListOfSize(2)
                                           .All()
                                           .With(e => e.AirDateUtc = System.DateTime.UtcNow.AddDays(+7))
                                           .With(s => s.SeriesId = show.Id)
                                           .BuildList(),
                Series = show,
                Release = new ReleaseInfo
                {
                    PublishDate = System.DateTime.Now
                }
            };
        }

        private void GivenGrabUnairedEpisodes(bool decision)
        {
            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.GrabUnairedReleases)
                  .Returns(decision);
        }

        [Test]
        public void should_grab_release_if_GrabUnairedEpisodes_is_true()
        {
            GivenGrabUnairedEpisodes(true);
            Subject.IsSatisfiedBy(_goodRemoteEpisode, null).Accepted.Should().BeTrue();
            Subject.IsSatisfiedBy(_fakeRemoteEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_grab_release_if_GrabUnairedEpisodes_is_false_and_release_is_good()
        {
            GivenGrabUnairedEpisodes(false);
            Subject.IsSatisfiedBy(_goodRemoteEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_not_grab_release_if_GrabUnairedEpisodes_is_false_and_release_is_fake()
        {
            GivenGrabUnairedEpisodes(false);
            Subject.IsSatisfiedBy(_fakeRemoteEpisode, null).Accepted.Should().BeFalse();
        }
    }
}
