// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using AutoMoq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class InventoryProvider_IsAcceptableSizeTest : TestBase
    {
        private EpisodeParseResult parseResultMulti;
        private EpisodeParseResult parseResultSingle;
        private Series series30minutes;
        private Series series60minutes;
        private QualityType qualityType;

        [SetUp]
        public new void Setup()
        {
            parseResultMulti = new EpisodeParseResult
                                   {
                                       CleanTitle = "Title",
                                       Language = LanguageType.English,
                                       Quality = new Quality(QualityTypes.SDTV, true),
                                       EpisodeNumbers = new List<int> { 3, 4 },
                                       SeasonNumber = 12,
                                       AirDate = DateTime.Now.AddDays(-12).Date
                                   };

            parseResultSingle = new EpisodeParseResult
                                    {
                                        CleanTitle = "Title",
                                        Language = LanguageType.English,
                                        Quality = new Quality(QualityTypes.SDTV, true),
                                        EpisodeNumbers = new List<int> { 3 },
                                        SeasonNumber = 12,
                                        AirDate = DateTime.Now.AddDays(-12).Date
                                    };

            series30minutes = Builder<Series>.CreateNew()
                .With(c => c.Monitored = true)
                .With(d => d.CleanTitle = parseResultMulti.CleanTitle)
                .With(c => c.Runtime = 30)
                .Build();

            series60minutes = Builder<Series>.CreateNew()
                .With(c => c.Monitored = true)
                .With(d => d.CleanTitle = parseResultMulti.CleanTitle)
                .With(c => c.Runtime = 60)
                .Build();

            qualityType = Builder<QualityType>.CreateNew()
                .With(q => q.MinSize = 0)
                .With(q => q.MaxSize = 314572800)
                .With(q => q.QualityTypeId = 1)
                .Build();

            base.Setup();
        }

        [Test]
        public void IsAcceptableSize_true_single_episode_not_first_or_last_30_minute()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);

            parseResultSingle.Series = series30minutes;
            parseResultSingle.Size = 184572800;

            mocker.GetMock<QualityTypeProvider>().Setup(s => s.Get(1)).Returns(qualityType);

            mocker.GetMock<EpisodeProvider>().Setup(
                s => s.IsFirstOrLastEpisodeOfSeason(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(false);

            //Act
            bool result = mocker.Resolve<InventoryProvider>().IsAcceptableSize(parseResultSingle);

            //Assert
            result.Should().BeTrue();
        }

        [Test]
        public void IsAcceptableSize_true_single_episode_not_first_or_last_60_minute()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);

            parseResultSingle.Series = series60minutes;
            parseResultSingle.Size = 368572800;

            mocker.GetMock<QualityTypeProvider>().Setup(s => s.Get(1)).Returns(qualityType);

            mocker.GetMock<EpisodeProvider>().Setup(
                s => s.IsFirstOrLastEpisodeOfSeason(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(false);

            //Act
            bool result = mocker.Resolve<InventoryProvider>().IsAcceptableSize(parseResultSingle);

            //Assert
            result.Should().BeTrue();
        }

        [Test]
        public void IsAcceptableSize_false_single_episode_not_first_or_last_30_minute()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);

            parseResultSingle.Series = series30minutes;
            parseResultSingle.Size = 1184572800;

            mocker.GetMock<QualityTypeProvider>().Setup(s => s.Get(1)).Returns(qualityType);

            mocker.GetMock<EpisodeProvider>().Setup(
                s => s.IsFirstOrLastEpisodeOfSeason(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(false);

            //Act
            bool result = mocker.Resolve<InventoryProvider>().IsAcceptableSize(parseResultSingle);

            //Assert
            result.Should().BeFalse();
        }

        [Test]
        public void IsAcceptableSize_false_single_episode_not_first_or_last_60_minute()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);

            parseResultSingle.Series = series60minutes;
            parseResultSingle.Size = 1368572800;

            mocker.GetMock<QualityTypeProvider>().Setup(s => s.Get(1)).Returns(qualityType);

            mocker.GetMock<EpisodeProvider>().Setup(
                s => s.IsFirstOrLastEpisodeOfSeason(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(false);

            //Act
            bool result = mocker.Resolve<InventoryProvider>().IsAcceptableSize(parseResultSingle);

            //Assert
            result.Should().BeFalse();
        }

        [Test]
        public void IsAcceptableSize_true_multi_episode_not_first_or_last_30_minute()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);

            parseResultMulti.Series = series30minutes;
            parseResultMulti.Size = 184572800;

            mocker.GetMock<QualityTypeProvider>().Setup(s => s.Get(1)).Returns(qualityType);

            mocker.GetMock<EpisodeProvider>().Setup(
                s => s.IsFirstOrLastEpisodeOfSeason(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(false);

            //Act
            bool result = mocker.Resolve<InventoryProvider>().IsAcceptableSize(parseResultMulti);

            //Assert
            result.Should().BeTrue();
        }

        [Test]
        public void IsAcceptableSize_true_multi_episode_not_first_or_last_60_minute()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);

            parseResultMulti.Series = series60minutes;
            parseResultMulti.Size = 368572800;

            mocker.GetMock<QualityTypeProvider>().Setup(s => s.Get(1)).Returns(qualityType);

            mocker.GetMock<EpisodeProvider>().Setup(
                s => s.IsFirstOrLastEpisodeOfSeason(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(false);

            //Act
            bool result = mocker.Resolve<InventoryProvider>().IsAcceptableSize(parseResultMulti);

            //Assert
            result.Should().BeTrue();
        }

        [Test]
        public void IsAcceptableSize_false_multi_episode_not_first_or_last_30_minute()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);

            parseResultMulti.Series = series30minutes;
            parseResultMulti.Size = 1184572800;

            mocker.GetMock<QualityTypeProvider>().Setup(s => s.Get(1)).Returns(qualityType);

            mocker.GetMock<EpisodeProvider>().Setup(
                s => s.IsFirstOrLastEpisodeOfSeason(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(false);

            //Act
            bool result = mocker.Resolve<InventoryProvider>().IsAcceptableSize(parseResultMulti);

            //Assert
            result.Should().BeFalse();
        }

        [Test]
        public void IsAcceptableSize_false_multi_episode_not_first_or_last_60_minute()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);

            parseResultMulti.Series = series60minutes;
            parseResultMulti.Size = 1368572800;

            mocker.GetMock<QualityTypeProvider>().Setup(s => s.Get(1)).Returns(qualityType);

            mocker.GetMock<EpisodeProvider>().Setup(
                s => s.IsFirstOrLastEpisodeOfSeason(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(false);

            //Act
            bool result = mocker.Resolve<InventoryProvider>().IsAcceptableSize(parseResultMulti);

            //Assert
            result.Should().BeFalse();
        }

        [Test]
        public void IsAcceptableSize_true_single_episode_first_30_minute()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);

            parseResultSingle.Series = series30minutes;
            parseResultSingle.Size = 184572800;

            mocker.GetMock<QualityTypeProvider>().Setup(s => s.Get(1)).Returns(qualityType);

            mocker.GetMock<EpisodeProvider>().Setup(
                s => s.IsFirstOrLastEpisodeOfSeason(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(true);

            //Act
            bool result = mocker.Resolve<InventoryProvider>().IsAcceptableSize(parseResultSingle);

            //Assert
            result.Should().BeTrue();
        }

        [Test]
        public void IsAcceptableSize_true_single_episode_first_60_minute()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);

            parseResultSingle.Series = series60minutes;
            parseResultSingle.Size = 368572800;

            mocker.GetMock<QualityTypeProvider>().Setup(s => s.Get(1)).Returns(qualityType);

            mocker.GetMock<EpisodeProvider>().Setup(
                s => s.IsFirstOrLastEpisodeOfSeason(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(true);

            //Act
            bool result = mocker.Resolve<InventoryProvider>().IsAcceptableSize(parseResultSingle);

            //Assert
            result.Should().BeTrue();
        }

        [Test]
        public void IsAcceptableSize_false_single_episode_first_30_minute()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);

            parseResultSingle.Series = series30minutes;
            parseResultSingle.Size = 1184572800;

            mocker.GetMock<QualityTypeProvider>().Setup(s => s.Get(1)).Returns(qualityType);

            mocker.GetMock<EpisodeProvider>().Setup(
                s => s.IsFirstOrLastEpisodeOfSeason(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(true);

            //Act
            bool result = mocker.Resolve<InventoryProvider>().IsAcceptableSize(parseResultSingle);

            //Assert
            result.Should().BeFalse();
        }

        [Test]
        public void IsAcceptableSize_false_single_episode_first_60_minute()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);

            parseResultSingle.Series = series60minutes;
            parseResultSingle.Size = 1368572800;

            mocker.GetMock<QualityTypeProvider>().Setup(s => s.Get(1)).Returns(qualityType);

            mocker.GetMock<EpisodeProvider>().Setup(
                s => s.IsFirstOrLastEpisodeOfSeason(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(true);

            //Act
            bool result = mocker.Resolve<InventoryProvider>().IsAcceptableSize(parseResultSingle);

            //Assert
            result.Should().BeFalse();
        }
    }
}