using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class SeriesEditionSpecificationFixture : TestBase<SeriesEditionSpecification>
    {
        [Test]
        public void should_reject_rezero_directors_cut_release_without_marker()
        {
            var remoteEpisode = new RemoteEpisode
            {
                Series = new Series
                {
                    TvdbId = 305089,
                    SeriesEdition = SeriesEditions.DirectorsCut
                },
                Release = new ReleaseInfo
                {
                    Title = "Re Zero S01E01 1080p WEB-DL"
                }
            };

            var decision = Subject.IsSatisfiedBy(remoteEpisode, new ReleaseDecisionInformation(false, null));

            decision.Accepted.Should().BeFalse();
            decision.Reason.Should().Be(DownloadRejectionReason.WrongSeriesEdition);
        }

        [Test]
        public void should_accept_rezero_directors_cut_release_with_marker()
        {
            var remoteEpisode = new RemoteEpisode
            {
                Series = new Series
                {
                    TvdbId = 305089,
                    SeriesEdition = SeriesEditions.DirectorsCut
                },
                Release = new ReleaseInfo
                {
                    Title = "Re Zero Directors Cut S01E01 1080p WEB-DL"
                }
            };

            Subject.IsSatisfiedBy(remoteEpisode, new ReleaseDecisionInformation(false, null)).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_reject_generic_directors_cut_release_without_marker()
        {
            var remoteEpisode = new RemoteEpisode
            {
                Series = new Series
                {
                    TvdbId = 1,
                    Title = "Some Show",
                    SeriesEdition = SeriesEditions.DirectorsCut
                },
                Release = new ReleaseInfo
                {
                    Title = "Some Show S01E01 1080p WEB-DL"
                }
            };

            var decision = Subject.IsSatisfiedBy(remoteEpisode, new ReleaseDecisionInformation(false, null));

            decision.Accepted.Should().BeFalse();
            decision.Reason.Should().Be(DownloadRejectionReason.WrongSeriesEdition);
        }

        [Test]
        public void should_accept_generic_directors_cut_release_with_marker()
        {
            var remoteEpisode = new RemoteEpisode
            {
                Series = new Series
                {
                    TvdbId = 1,
                    Title = "Some Show",
                    SeriesEdition = SeriesEditions.DirectorsCut
                },
                Release = new ReleaseInfo
                {
                    Title = "Some Show Directors Cut S01E01 1080p WEB-DL"
                }
            };

            Subject.IsSatisfiedBy(remoteEpisode, new ReleaseDecisionInformation(false, null)).Accepted.Should().BeTrue();
        }
    }
}
