// ReSharper disable RedundantUsingDirective

using System.Collections.Generic;

using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.ProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class QualityTypeProviderTest : SqlCeTest
    {
        [SetUp]
        public void SetuUp()
        {
            WithRealDb();
        }

        [Test]
        public void SetupDefault_should_add_all_profiles()
        {
            Mocker.Resolve<QualityTypeProvider>();

            
            var types = Mocker.Resolve<QualityTypeProvider>().All();

            types.Should().HaveCount(10);
            types.Should().Contain(e => e.Name == "SDTV" && e.QualityTypeId == 1);
            types.Should().Contain(e => e.Name == "DVD" && e.QualityTypeId == 2);
            types.Should().Contain(e => e.Name == "WEBDL-480p" && e.QualityTypeId == 8);
            types.Should().Contain(e => e.Name == "HDTV-720p" && e.QualityTypeId == 4);
            types.Should().Contain(e => e.Name == "HDTV-1080p" && e.QualityTypeId == 9);
            types.Should().Contain(e => e.Name == "Raw-HD" && e.QualityTypeId == 10);
            types.Should().Contain(e => e.Name == "WEBDL-720p" && e.QualityTypeId == 5);
            types.Should().Contain(e => e.Name == "WEBDL-1080p" && e.QualityTypeId == 3);
            types.Should().Contain(e => e.Name == "Bluray720p" && e.QualityTypeId == 6);
            types.Should().Contain(e => e.Name == "Bluray1080p" && e.QualityTypeId == 7);
        }

        [Test]
        public void SetupDefault_already_exists_should_insert_missing()
        {

            Db.Insert(new QualityType { QualityTypeId = 1, Name = "SDTV", MinSize = 0, MaxSize = 100 });

            
            Mocker.Resolve<QualityTypeProvider>();

            
            var types = Mocker.Resolve<QualityTypeProvider>().All();

            types.Should().HaveCount(QualityTypes.All().Count - 1);
        }

        [Test]
        public void GetList_single_quality_type()
        {
            var fakeQualityTypes = Builder<QualityType>.CreateListOfSize(6)
                .Build();

            var ids = new List<int> { 1 };

            Db.InsertMany(fakeQualityTypes);

            
            var result = Mocker.Resolve<QualityTypeProvider>().GetList(ids);

            
            result.Should().HaveCount(ids.Count);
        }

        [Test]
        public void GetList_multiple_quality_type()
        {
            var fakeQualityTypes = Builder<QualityType>.CreateListOfSize(6)
                .Build();

            var ids = new List<int> { 1, 2 };

            Db.InsertMany(fakeQualityTypes);

            
            var result = Mocker.Resolve<QualityTypeProvider>().GetList(ids);

            
            result.Should().HaveCount(ids.Count);
        }
    }
}