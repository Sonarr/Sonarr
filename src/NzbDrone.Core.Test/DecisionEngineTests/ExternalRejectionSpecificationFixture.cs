using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.DecisionEngine.ExternalDecisions;
using NzbDrone.Core.DecisionEngine.ExternalDecisions.Payloads;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

using CustomFormat = NzbDrone.Core.CustomFormats.CustomFormat;
using Language = NzbDrone.Core.Languages.Language;
using QualityModel = NzbDrone.Core.Qualities.QualityModel;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class ExternalRejectionSpecificationFixture : CoreTest<ExternalRejectionSpecification>
    {
        private RemoteEpisode _remoteEpisode;
        private Mock<IExternalDecision> _decisionMock;
        private ExternalDecisionDefinition _decisionDefinition;

        [SetUp]
        public void Setup()
        {
            _remoteEpisode = new RemoteEpisode
            {
                Series = new Series
                {
                    Id = 1,
                    TvdbId = 123456,
                    Title = "Test Series",
                    Tags = new HashSet<int> { 1, 2 },
                    QualityProfileId = 1
                },
                Release = new ReleaseInfo
                {
                    Guid = "test-guid",
                    Title = "Test.Series.S01E01.1080p.WEB-DL",
                    Indexer = "TestIndexer",
                    Size = 1073741824,
                    IndexerPriority = 25
                },
                ParsedEpisodeInfo = new ParsedEpisodeInfo
                {
                    Quality = new QualityModel()
                },
                Episodes = new List<Episode>
                {
                    new Episode
                    {
                        Id = 100,
                        SeasonNumber = 1,
                        EpisodeNumber = 1,
                        Title = "Pilot"
                    }
                },
                CustomFormats = new List<CustomFormat>(),
                Languages = new List<Language>()
            };

            _decisionDefinition = new ExternalDecisionDefinition
            {
                Id = 1,
                Name = "Test Decision",
                DecisionType = ExternalDecisionType.Rejection,
                Tags = new HashSet<int>(),
                Enable = true
            };

            _decisionMock = new Mock<IExternalDecision>();
            _decisionMock.SetupGet(h => h.Definition).Returns(_decisionDefinition);

            Mocker.GetMock<IExternalDecisionFactory>()
                  .Setup(f => f.RejectionDecisionsEnabled())
                  .Returns(new List<IExternalDecision>());
        }

        private void GivenHookApproves()
        {
            _decisionMock.Setup(h => h.EvaluateRejection(It.IsAny<ExternalRejectionRequest>()))
                     .Returns(new ExternalRejectionResponse { Approved = true });

            Mocker.GetMock<IExternalDecisionFactory>()
                  .Setup(f => f.RejectionDecisionsEnabled())
                  .Returns(new List<IExternalDecision> { _decisionMock.Object });
        }

        private void GivenHookRejects(string reason)
        {
            _decisionMock.Setup(h => h.EvaluateRejection(It.IsAny<ExternalRejectionRequest>()))
                     .Returns(new ExternalRejectionResponse { Approved = false, Reason = reason });

            Mocker.GetMock<IExternalDecisionFactory>()
                  .Setup(f => f.RejectionDecisionsEnabled())
                  .Returns(new List<IExternalDecision> { _decisionMock.Object });
        }

        private void GivenHookThrows()
        {
            _decisionMock.Setup(h => h.EvaluateRejection(It.IsAny<ExternalRejectionRequest>()))
                     .Throws(new Exception("Connection timeout"));

            Mocker.GetMock<IExternalDecisionFactory>()
                  .Setup(f => f.RejectionDecisionsEnabled())
                  .Returns(new List<IExternalDecision> { _decisionMock.Object });
        }

        [Test]
        public void should_accept_when_no_decisions_enabled()
        {
            Subject.IsSatisfiedBy(_remoteEpisode, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_when_decision_approves_release()
        {
            GivenHookApproves();

            Subject.IsSatisfiedBy(_remoteEpisode, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_reject_when_decision_rejects_release()
        {
            GivenHookRejects("Release contains unwanted format");

            Subject.IsSatisfiedBy(_remoteEpisode, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_accept_when_decision_times_out()
        {
            GivenHookThrows();

            Subject.IsSatisfiedBy(_remoteEpisode, new()).Accepted.Should().BeTrue();

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_accept_when_decision_returns_error()
        {
            GivenHookThrows();

            var result = Subject.IsSatisfiedBy(_remoteEpisode, new());

            result.Accepted.Should().BeTrue();

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_skip_decision_when_tags_dont_match()
        {
            _decisionDefinition.Tags = new HashSet<int> { 99 };

            _decisionMock.Setup(h => h.EvaluateRejection(It.IsAny<ExternalRejectionRequest>()))
                     .Returns(new ExternalRejectionResponse { Approved = false, Reason = "Should not reach" });

            Mocker.GetMock<IExternalDecisionFactory>()
                  .Setup(f => f.RejectionDecisionsEnabled())
                  .Returns(new List<IExternalDecision> { _decisionMock.Object });

            Subject.IsSatisfiedBy(_remoteEpisode, new()).Accepted.Should().BeTrue();

            _decisionMock.Verify(h => h.EvaluateRejection(It.IsAny<ExternalRejectionRequest>()), Times.Never);
        }

        [Test]
        public void should_apply_decision_when_decision_has_no_tags()
        {
            _decisionDefinition.Tags = new HashSet<int>();

            GivenHookRejects("No tags means applies to all");

            Subject.IsSatisfiedBy(_remoteEpisode, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_apply_decision_when_tags_intersect()
        {
            _decisionDefinition.Tags = new HashSet<int> { 2, 5 };

            GivenHookRejects("Matching tag 2");

            Subject.IsSatisfiedBy(_remoteEpisode, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_reject_when_any_decision_rejects()
        {
            var decision1 = new Mock<IExternalDecision>();
            var definition1 = new ExternalDecisionDefinition
            {
                Id = 1,
                Name = "Decision 1",
                DecisionType = ExternalDecisionType.Rejection,
                Tags = new HashSet<int>(),
                Enable = true
            };
            decision1.SetupGet(h => h.Definition).Returns(definition1);
            decision1.Setup(h => h.EvaluateRejection(It.IsAny<ExternalRejectionRequest>()))
                 .Returns(new ExternalRejectionResponse { Approved = true });

            var decision2 = new Mock<IExternalDecision>();
            var definition2 = new ExternalDecisionDefinition
            {
                Id = 2,
                Name = "Decision 2",
                DecisionType = ExternalDecisionType.Rejection,
                Tags = new HashSet<int>(),
                Enable = true
            };
            decision2.SetupGet(h => h.Definition).Returns(definition2);
            decision2.Setup(h => h.EvaluateRejection(It.IsAny<ExternalRejectionRequest>()))
                 .Returns(new ExternalRejectionResponse { Approved = false, Reason = "Rejected by decision 2" });

            Mocker.GetMock<IExternalDecisionFactory>()
                  .Setup(f => f.RejectionDecisionsEnabled())
                  .Returns(new List<IExternalDecision> { decision1.Object, decision2.Object });

            var result = Subject.IsSatisfiedBy(_remoteEpisode, new());

            result.Accepted.Should().BeFalse();
            decision1.Verify(h => h.EvaluateRejection(It.IsAny<ExternalRejectionRequest>()), Times.Once);
            decision2.Verify(h => h.EvaluateRejection(It.IsAny<ExternalRejectionRequest>()), Times.Once);
        }

        [Test]
        public void should_include_reason_from_hook_response()
        {
            GivenHookRejects("RAR archive detected");

            var result = Subject.IsSatisfiedBy(_remoteEpisode, new());

            result.Accepted.Should().BeFalse();
            result.Reason.Should().Be(DownloadRejectionReason.ExternalRejection);
            result.Message.Should().Be("External: RAR archive detected");
        }

        [Test]
        public void should_have_external_priority()
        {
            Subject.Priority.Should().Be(SpecificationPriority.External);
        }

        [Test]
        public void should_record_failure_on_exception()
        {
            GivenHookThrows();

            Subject.IsSatisfiedBy(_remoteEpisode, new());

            Mocker.GetMock<IExternalDecisionStatusService>()
                  .Verify(s => s.RecordFailure(1), Times.Once);

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_record_success_on_approval()
        {
            GivenHookApproves();

            Subject.IsSatisfiedBy(_remoteEpisode, new());

            Mocker.GetMock<IExternalDecisionStatusService>()
                  .Verify(s => s.RecordSuccess(1), Times.Once);
        }

        [Test]
        public void should_include_release_and_series_data_in_request()
        {
            ExternalRejectionRequest capturedRequest = null;

            _decisionMock.Setup(h => h.EvaluateRejection(It.IsAny<ExternalRejectionRequest>()))
                     .Callback<ExternalRejectionRequest>(r => capturedRequest = r)
                     .Returns(new ExternalRejectionResponse { Approved = true });

            Mocker.GetMock<IExternalDecisionFactory>()
                  .Setup(f => f.RejectionDecisionsEnabled())
                  .Returns(new List<IExternalDecision> { _decisionMock.Object });

            Subject.IsSatisfiedBy(_remoteEpisode, new());

            capturedRequest.Should().NotBeNull();
            capturedRequest.DecisionType.Should().Be("Rejection");
            capturedRequest.Release.Should().NotBeNull();
            capturedRequest.Release.Guid.Should().Be("test-guid");
            capturedRequest.Release.Title.Should().Be("Test.Series.S01E01.1080p.WEB-DL");
            capturedRequest.Series.Should().NotBeNull();
            capturedRequest.Series.Id.Should().Be(1);
            capturedRequest.Series.TvdbId.Should().Be(123456);
            capturedRequest.Episodes.Should().HaveCount(1);
            capturedRequest.Episodes[0].SeasonNumber.Should().Be(1);
            capturedRequest.Episodes[0].EpisodeNumber.Should().Be(1);
        }

        [Test]
        public void should_skip_decision_when_series_has_no_tags_and_hook_has_tags()
        {
            _decisionDefinition.Tags = new HashSet<int> { 1 };
            _remoteEpisode.Series.Tags = new HashSet<int>();

            GivenHookRejects("Should not reach");

            Subject.IsSatisfiedBy(_remoteEpisode, new()).Accepted.Should().BeTrue();

            _decisionMock.Verify(h => h.EvaluateRejection(It.IsAny<ExternalRejectionRequest>()), Times.Never);
        }

        [Test]
        public void should_record_success_on_rejection()
        {
            GivenHookRejects("Some reason");

            Subject.IsSatisfiedBy(_remoteEpisode, new());

            Mocker.GetMock<IExternalDecisionStatusService>()
                  .Verify(s => s.RecordSuccess(1), Times.Once);
        }
    }
}
