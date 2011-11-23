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
    public class QualityTypeProviderTest : CoreTest
    {
        [Test]
        public void SetupDefault_should_add_six_profiles()
        {
            var mocker = new AutoMoqer();
            var db = TestDbHelper.GetEmptyDatabase();
            mocker.SetConstant(db);

            //Act
            mocker.Resolve<QualityTypeProvider>().SetupDefault();

            //Assert
            var types = mocker.Resolve<QualityTypeProvider>().All();

            types.Should().HaveCount(6);
            types.Should().Contain(e => e.Name == "SDTV" && e.QualityTypeId == 1);
            types.Should().Contain(e => e.Name == "DVD" && e.QualityTypeId == 2);
            types.Should().Contain(e => e.Name == "HDTV" && e.QualityTypeId == 4);
            types.Should().Contain(e => e.Name == "WEBDL" && e.QualityTypeId == 5);
            types.Should().Contain(e => e.Name == "Bluray720p" && e.QualityTypeId == 6);
            types.Should().Contain(e => e.Name == "Bluray1080p" && e.QualityTypeId == 7);
        }

        [Test]
        public void SetupDefault_already_exists()
        {
            var mocker = new AutoMoqer();
            var db = TestDbHelper.GetEmptyDatabase();
            mocker.SetConstant(db);

            var fakeQualityType = Builder<QualityType>.CreateNew()
                .Build();

            db.Insert(fakeQualityType);

            //Act
            mocker.Resolve<QualityTypeProvider>().SetupDefault();

            //Assert
            var types = mocker.Resolve<QualityTypeProvider>().All();

            types.Should().HaveCount(1);
        }

        [Test]
        public void GetList_single_quality_type()
        {
            var mocker = new AutoMoqer();
            var db = TestDbHelper.GetEmptyDatabase();
            mocker.SetConstant(db);

            var fakeQualityTypes = Builder<QualityType>.CreateListOfSize(6)
                .Build();

            var ids = new List<int> { 1 };

            db.InsertMany(fakeQualityTypes);

            //Act
            var result = mocker.Resolve<QualityTypeProvider>().GetList(ids);

            //Assert
            result.Should().HaveCount(ids.Count);
        }

        [Test]
        public void GetList_multiple_quality_type()
        {
            var mocker = new AutoMoqer();
            var db = TestDbHelper.GetEmptyDatabase();
            mocker.SetConstant(db);

            var fakeQualityTypes = Builder<QualityType>.CreateListOfSize(6)
                .Build();

            var ids = new List<int> { 1, 2 };

            db.InsertMany(fakeQualityTypes);

            //Act
            var result = mocker.Resolve<QualityTypeProvider>().GetList(ids);

            //Assert
            result.Should().HaveCount(ids.Count);
        }
    }
}