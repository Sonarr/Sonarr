using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.DecisionEngine.ExternalDecisions;
using NzbDrone.Core.DecisionEngine.ExternalDecisions.Payloads;
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
    public class ExternalPrioritizationServiceFixture : CoreTest<ExternalPrioritizationService>
    {
        private List<DownloadDecision> _decisions;
        private Series _series;
        private Mock<IExternalDecision> _decisionMock;
        private ExternalDecisionDefinition _decisionDefinition;

        [SetUp]
        public void Setup()
        {
            _series = new Series
            {
                Id = 1,
                TvdbId = 123456,
                Title = "Test Series",
                Tags = new HashSet<int> { 1, 2 },
                QualityProfileId = 1
            };

            _decisions = new List<DownloadDecision>
            {
                CreateDecision("guid-1", "Release.A.S01E01.1080p.WEB-DL"),
                CreateDecision("guid-2", "Release.B.S01E01.720p.HDTV"),
                CreateDecision("guid-3", "Release.C.S01E01.1080p.Bluray")
            };

            _decisionDefinition = new ExternalDecisionDefinition
            {
                Id = 1,
                Name = "Test Prioritization Decision",
                DecisionType = ExternalDecisionType.Prioritization,
                Tags = new HashSet<int>(),
                Enable = true
            };

            _decisionMock = new Mock<IExternalDecision>();
            _decisionMock.SetupGet(h => h.Definition).Returns(_decisionDefinition);

            Mocker.GetMock<IExternalDecisionFactory>()
                  .Setup(f => f.PrioritizationDecisionsEnabled())
                  .Returns(new List<IExternalDecision>());
        }

        private DownloadDecision CreateDecision(string guid, string title)
        {
            var remoteEpisode = new RemoteEpisode
            {
                Series = _series,
                Release = new ReleaseInfo
                {
                    Guid = guid,
                    Title = title,
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

            return new DownloadDecision(remoteEpisode);
        }

        private void GivenDecisionReturnsScores(Dictionary<string, int> scores)
        {
            _decisionMock.Setup(h => h.EvaluatePrioritization(It.IsAny<ExternalPrioritizationRequest>()))
                     .Returns(new ExternalPrioritizationResponse { Scores = scores });

            Mocker.GetMock<IExternalDecisionFactory>()
                  .Setup(f => f.PrioritizationDecisionsEnabled())
                  .Returns(new List<IExternalDecision> { _decisionMock.Object });
        }

        private void GivenDecisionReturnsEmpty()
        {
            _decisionMock.Setup(h => h.EvaluatePrioritization(It.IsAny<ExternalPrioritizationRequest>()))
                     .Returns(new ExternalPrioritizationResponse { Scores = new Dictionary<string, int>() });

            Mocker.GetMock<IExternalDecisionFactory>()
                  .Setup(f => f.PrioritizationDecisionsEnabled())
                  .Returns(new List<IExternalDecision> { _decisionMock.Object });
        }

        private void GivenDecisionReturnsNull()
        {
            _decisionMock.Setup(h => h.EvaluatePrioritization(It.IsAny<ExternalPrioritizationRequest>()))
                     .Returns((ExternalPrioritizationResponse)null);

            Mocker.GetMock<IExternalDecisionFactory>()
                  .Setup(f => f.PrioritizationDecisionsEnabled())
                  .Returns(new List<IExternalDecision> { _decisionMock.Object });
        }

        private void GivenDecisionThrows()
        {
            _decisionMock.Setup(h => h.EvaluatePrioritization(It.IsAny<ExternalPrioritizationRequest>()))
                     .Throws(new Exception("Connection timeout"));

            Mocker.GetMock<IExternalDecisionFactory>()
                  .Setup(f => f.PrioritizationDecisionsEnabled())
                  .Returns(new List<IExternalDecision> { _decisionMock.Object });
        }

        [Test]
        public void should_not_set_scores_when_no_decisions_enabled()
        {
            Subject.PopulateExternalPriorityScores(_decisions);

            _decisions.Should().OnlyContain(d => d.RemoteEpisode.ExternalPriorityScore == 0);
        }

        [Test]
        public void should_assign_scores_from_decision_response()
        {
            GivenDecisionReturnsScores(new Dictionary<string, int>
            {
                { "guid-3", 100 },
                { "guid-1", 50 },
                { "guid-2", 75 }
            });

            Subject.PopulateExternalPriorityScores(_decisions);

            _decisions.Single(d => d.RemoteEpisode.Release.Guid == "guid-3").RemoteEpisode.ExternalPriorityScore.Should().Be(100);
            _decisions.Single(d => d.RemoteEpisode.Release.Guid == "guid-1").RemoteEpisode.ExternalPriorityScore.Should().Be(50);
            _decisions.Single(d => d.RemoteEpisode.Release.Guid == "guid-2").RemoteEpisode.ExternalPriorityScore.Should().Be(75);

            _decisionMock.Verify(h => h.EvaluatePrioritization(It.IsAny<ExternalPrioritizationRequest>()), Times.Once);
        }

        [Test]
        public void should_keep_zero_scores_when_decision_returns_empty()
        {
            GivenDecisionReturnsEmpty();

            Subject.PopulateExternalPriorityScores(_decisions);

            _decisions.Should().OnlyContain(d => d.RemoteEpisode.ExternalPriorityScore == 0);
        }

        [Test]
        public void should_keep_zero_scores_when_decision_returns_null()
        {
            GivenDecisionReturnsNull();

            Subject.PopulateExternalPriorityScores(_decisions);

            _decisions.Should().OnlyContain(d => d.RemoteEpisode.ExternalPriorityScore == 0);
        }

        [Test]
        public void should_keep_zero_scores_on_decision_exception()
        {
            GivenDecisionThrows();

            Subject.PopulateExternalPriorityScores(_decisions);

            _decisions.Should().OnlyContain(d => d.RemoteEpisode.ExternalPriorityScore == 0);

            _decisionMock.Verify(h => h.EvaluatePrioritization(It.IsAny<ExternalPrioritizationRequest>()), Times.Once);

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_record_failure_on_exception()
        {
            GivenDecisionThrows();

            Subject.PopulateExternalPriorityScores(_decisions);

            Mocker.GetMock<IExternalDecisionStatusService>()
                  .Verify(s => s.RecordFailure(1), Times.Once);

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_record_success_on_score_assignment()
        {
            GivenDecisionReturnsScores(new Dictionary<string, int> { { "guid-3", 100 }, { "guid-1", 50 }, { "guid-2", 75 } });

            Subject.PopulateExternalPriorityScores(_decisions);

            Mocker.GetMock<IExternalDecisionStatusService>()
                  .Verify(s => s.RecordSuccess(1), Times.Once);
        }

        [Test]
        public void should_assign_zero_score_to_guids_missing_from_response()
        {
            GivenDecisionReturnsScores(new Dictionary<string, int> { { "guid-3", 42 } });

            Subject.PopulateExternalPriorityScores(_decisions);

            _decisions.Single(d => d.RemoteEpisode.Release.Guid == "guid-3").RemoteEpisode.ExternalPriorityScore.Should().Be(42);
            _decisions.Single(d => d.RemoteEpisode.Release.Guid == "guid-1").RemoteEpisode.ExternalPriorityScore.Should().Be(0);
            _decisions.Single(d => d.RemoteEpisode.Release.Guid == "guid-2").RemoteEpisode.ExternalPriorityScore.Should().Be(0);
        }

        [Test]
        public void should_ignore_unknown_guids_in_response()
        {
            GivenDecisionReturnsScores(new Dictionary<string, int>
            {
                { "unknown-guid", 999 },
                { "guid-2", 80 },
                { "guid-1", 60 }
            });

            Subject.PopulateExternalPriorityScores(_decisions);

            _decisions.Single(d => d.RemoteEpisode.Release.Guid == "guid-2").RemoteEpisode.ExternalPriorityScore.Should().Be(80);
            _decisions.Single(d => d.RemoteEpisode.Release.Guid == "guid-1").RemoteEpisode.ExternalPriorityScore.Should().Be(60);
            _decisions.Single(d => d.RemoteEpisode.Release.Guid == "guid-3").RemoteEpisode.ExternalPriorityScore.Should().Be(0);
        }

        [Test]
        public void should_chain_multiple_decisions_with_last_decision_scores_winning()
        {
            var decision1 = new Mock<IExternalDecision>();
            var definition1 = new ExternalDecisionDefinition
            {
                Id = 1,
                Name = "Decision 1",
                DecisionType = ExternalDecisionType.Prioritization,
                Tags = new HashSet<int>(),
                Enable = true
            };
            decision1.SetupGet(h => h.Definition).Returns(definition1);
            decision1.Setup(h => h.EvaluatePrioritization(It.IsAny<ExternalPrioritizationRequest>()))
                 .Returns(new ExternalPrioritizationResponse { Scores = new Dictionary<string, int> { { "guid-3", 100 }, { "guid-2", 50 }, { "guid-1", 25 } } });

            var decision2 = new Mock<IExternalDecision>();
            var definition2 = new ExternalDecisionDefinition
            {
                Id = 2,
                Name = "Decision 2",
                DecisionType = ExternalDecisionType.Prioritization,
                Tags = new HashSet<int>(),
                Enable = true
            };
            decision2.SetupGet(h => h.Definition).Returns(definition2);
            decision2.Setup(h => h.EvaluatePrioritization(It.IsAny<ExternalPrioritizationRequest>()))
                 .Returns(new ExternalPrioritizationResponse { Scores = new Dictionary<string, int> { { "guid-1", 200 }, { "guid-3", 150 }, { "guid-2", 10 } } });

            Mocker.GetMock<IExternalDecisionFactory>()
                  .Setup(f => f.PrioritizationDecisionsEnabled())
                  .Returns(new List<IExternalDecision> { decision1.Object, decision2.Object });

            Subject.PopulateExternalPriorityScores(_decisions);

            // Decision 2's scores should be final (overwrites decision 1)
            _decisions.Single(d => d.RemoteEpisode.Release.Guid == "guid-1").RemoteEpisode.ExternalPriorityScore.Should().Be(200);
            _decisions.Single(d => d.RemoteEpisode.Release.Guid == "guid-3").RemoteEpisode.ExternalPriorityScore.Should().Be(150);
            _decisions.Single(d => d.RemoteEpisode.Release.Guid == "guid-2").RemoteEpisode.ExternalPriorityScore.Should().Be(10);
        }

        [Test]
        public void should_not_set_scores_on_decisions_without_series()
        {
            var noSeriesDecision = new DownloadDecision(new RemoteEpisode
            {
                Series = null,
                Release = new ReleaseInfo { Guid = "guid-noseries", Title = "Unknown" },
                ParsedEpisodeInfo = new ParsedEpisodeInfo { Quality = new QualityModel() },
                Episodes = new List<Episode>(),
                CustomFormats = new List<CustomFormat>(),
                Languages = new List<Language>()
            });

            var allDecisions = _decisions.Concat(new[] { noSeriesDecision }).ToList();

            GivenDecisionReturnsScores(new Dictionary<string, int> { { "guid-3", 100 }, { "guid-1", 50 }, { "guid-2", 75 } });

            Subject.PopulateExternalPriorityScores(allDecisions);

            noSeriesDecision.RemoteEpisode.ExternalPriorityScore.Should().Be(0);
            _decisions.Single(d => d.RemoteEpisode.Release.Guid == "guid-3").RemoteEpisode.ExternalPriorityScore.Should().Be(100);
        }

        [Test]
        public void should_send_all_releases_in_request()
        {
            ExternalPrioritizationRequest capturedRequest = null;

            _decisionMock.Setup(h => h.EvaluatePrioritization(It.IsAny<ExternalPrioritizationRequest>()))
                     .Callback<ExternalPrioritizationRequest>(r => capturedRequest = r)
                     .Returns(new ExternalPrioritizationResponse { Scores = new Dictionary<string, int>() });

            Mocker.GetMock<IExternalDecisionFactory>()
                  .Setup(f => f.PrioritizationDecisionsEnabled())
                  .Returns(new List<IExternalDecision> { _decisionMock.Object });

            Subject.PopulateExternalPriorityScores(_decisions);

            capturedRequest.Should().NotBeNull();
            capturedRequest.DecisionType.Should().Be("Prioritization");
            capturedRequest.Releases.Should().HaveCount(3);
            capturedRequest.Series.Should().NotBeNull();
            capturedRequest.Series.Id.Should().Be(1);
        }

        [Test]
        public void should_continue_chain_when_first_decision_throws()
        {
            var decision1 = new Mock<IExternalDecision>();
            var definition1 = new ExternalDecisionDefinition
            {
                Id = 1,
                Name = "Decision 1",
                DecisionType = ExternalDecisionType.Prioritization,
                Tags = new HashSet<int>(),
                Enable = true
            };
            decision1.SetupGet(h => h.Definition).Returns(definition1);
            decision1.Setup(h => h.EvaluatePrioritization(It.IsAny<ExternalPrioritizationRequest>()))
                 .Throws(new Exception("Decision 1 failure"));

            var decision2 = new Mock<IExternalDecision>();
            var definition2 = new ExternalDecisionDefinition
            {
                Id = 2,
                Name = "Decision 2",
                DecisionType = ExternalDecisionType.Prioritization,
                Tags = new HashSet<int>(),
                Enable = true
            };
            decision2.SetupGet(h => h.Definition).Returns(definition2);
            decision2.Setup(h => h.EvaluatePrioritization(It.IsAny<ExternalPrioritizationRequest>()))
                 .Returns(new ExternalPrioritizationResponse { Scores = new Dictionary<string, int> { { "guid-3", 100 }, { "guid-1", 50 }, { "guid-2", 75 } } });

            Mocker.GetMock<IExternalDecisionFactory>()
                  .Setup(f => f.PrioritizationDecisionsEnabled())
                  .Returns(new List<IExternalDecision> { decision1.Object, decision2.Object });

            Subject.PopulateExternalPriorityScores(_decisions);

            // Decision 2 should still run and set scores despite Decision 1 failure
            _decisions.Single(d => d.RemoteEpisode.Release.Guid == "guid-3").RemoteEpisode.ExternalPriorityScore.Should().Be(100);
            _decisions.Single(d => d.RemoteEpisode.Release.Guid == "guid-1").RemoteEpisode.ExternalPriorityScore.Should().Be(50);
            _decisions.Single(d => d.RemoteEpisode.Release.Guid == "guid-2").RemoteEpisode.ExternalPriorityScore.Should().Be(75);

            decision1.Verify(h => h.EvaluatePrioritization(It.IsAny<ExternalPrioritizationRequest>()), Times.Once);
            decision2.Verify(h => h.EvaluatePrioritization(It.IsAny<ExternalPrioritizationRequest>()), Times.Once);

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_skip_decision_when_tags_dont_match()
        {
            _decisionDefinition.Tags = new HashSet<int> { 99 };

            _decisionMock.Setup(h => h.EvaluatePrioritization(It.IsAny<ExternalPrioritizationRequest>()))
                     .Returns(new ExternalPrioritizationResponse { Scores = new Dictionary<string, int> { { "guid-3", 100 }, { "guid-1", 50 }, { "guid-2", 75 } } });

            Mocker.GetMock<IExternalDecisionFactory>()
                  .Setup(f => f.PrioritizationDecisionsEnabled())
                  .Returns(new List<IExternalDecision> { _decisionMock.Object });

            Subject.PopulateExternalPriorityScores(_decisions);

            _decisions.Should().OnlyContain(d => d.RemoteEpisode.ExternalPriorityScore == 0);

            _decisionMock.Verify(h => h.EvaluatePrioritization(It.IsAny<ExternalPrioritizationRequest>()), Times.Never);
        }

        [Test]
        public void should_apply_decision_when_decision_has_no_tags()
        {
            _decisionDefinition.Tags = new HashSet<int>();

            GivenDecisionReturnsScores(new Dictionary<string, int> { { "guid-3", 100 }, { "guid-1", 50 }, { "guid-2", 75 } });

            Subject.PopulateExternalPriorityScores(_decisions);

            _decisions.Single(d => d.RemoteEpisode.Release.Guid == "guid-3").RemoteEpisode.ExternalPriorityScore.Should().Be(100);

            _decisionMock.Verify(h => h.EvaluatePrioritization(It.IsAny<ExternalPrioritizationRequest>()), Times.Once);
        }

        [Test]
        public void should_apply_decision_when_tags_intersect()
        {
            _decisionDefinition.Tags = new HashSet<int> { 2, 5 };

            GivenDecisionReturnsScores(new Dictionary<string, int> { { "guid-3", 100 }, { "guid-1", 50 }, { "guid-2", 75 } });

            Subject.PopulateExternalPriorityScores(_decisions);

            _decisions.Single(d => d.RemoteEpisode.Release.Guid == "guid-3").RemoteEpisode.ExternalPriorityScore.Should().Be(100);

            _decisionMock.Verify(h => h.EvaluatePrioritization(It.IsAny<ExternalPrioritizationRequest>()), Times.Once);
        }

        [Test]
        public void should_skip_decision_when_series_has_no_tags_and_decision_has_tags()
        {
            _decisionDefinition.Tags = new HashSet<int> { 1 };
            _series.Tags = new HashSet<int>();

            _decisionMock.Setup(h => h.EvaluatePrioritization(It.IsAny<ExternalPrioritizationRequest>()))
                     .Returns(new ExternalPrioritizationResponse { Scores = new Dictionary<string, int> { { "guid-3", 100 }, { "guid-1", 50 }, { "guid-2", 75 } } });

            Mocker.GetMock<IExternalDecisionFactory>()
                  .Setup(f => f.PrioritizationDecisionsEnabled())
                  .Returns(new List<IExternalDecision> { _decisionMock.Object });

            Subject.PopulateExternalPriorityScores(_decisions);

            _decisions.Should().OnlyContain(d => d.RemoteEpisode.ExternalPriorityScore == 0);

            _decisionMock.Verify(h => h.EvaluatePrioritization(It.IsAny<ExternalPrioritizationRequest>()), Times.Never);
        }
    }
}
