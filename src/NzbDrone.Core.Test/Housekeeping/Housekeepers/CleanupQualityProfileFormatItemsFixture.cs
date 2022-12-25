using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupQualityProfileFormatItemsFixture : DbTest<CleanupQualityProfileFormatItems, QualityProfile>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.SetConstant<IQualityProfileFormatItemsCleanupRepository>(
                new QualityProfileFormatItemsCleanupRepository(Mocker.Resolve<IMainDatabase>(), Mocker.Resolve<IEventAggregator>()));
        }

        [Test]
        public void should_remove_orphaned_custom_formats()
        {
            var qualityProfile = Builder<QualityProfile>.CreateNew()
                .With(h => h.Items = Qualities.QualityFixture.GetDefaultQualities())
                .With(h => h.MinFormatScore = 50)
                .With(h => h.CutoffFormatScore = 100)
                .With(h => h.FormatItems = Builder<ProfileFormatItem>.CreateListOfSize(1).Build().ToList())
                .BuildNew();

            Db.Insert(qualityProfile);
            Subject.Clean();

            var result = AllStoredModels;

            result.Should().HaveCount(1);
            result.First().FormatItems.Should().BeEmpty();
            result.First().MinFormatScore.Should().Be(0);
            result.First().CutoffFormatScore.Should().Be(0);
        }

        [Test]
        public void should_not_remove_unorphaned_custom_formats()
        {
            var minFormatScore = 50;
            var cutoffFormatScore = 100;

            var customFormat = Builder<CustomFormat>.CreateNew()
                .With(h => h.Specifications = new List<ICustomFormatSpecification>())
                .BuildNew();

            Db.Insert(customFormat);

            var qualityProfile = Builder<QualityProfile>.CreateNew()
                .With(h => h.Items = Qualities.QualityFixture.GetDefaultQualities())
                .With(h => h.MinFormatScore = minFormatScore)
                .With(h => h.CutoffFormatScore = cutoffFormatScore)
                .With(h => h.FormatItems = new List<ProfileFormatItem>
                {
                    Builder<ProfileFormatItem>.CreateNew().With(f => f.Id = customFormat.Id).Build()
                })

                .BuildNew();

            Db.Insert(qualityProfile);

            Subject.Clean();
            var result = AllStoredModels;

            result.Should().HaveCount(1);
            result.First().FormatItems.Should().HaveCount(1);
            result.First().MinFormatScore.Should().Be(minFormatScore);
            result.First().CutoffFormatScore.Should().Be(cutoffFormatScore);
        }
    }
}
