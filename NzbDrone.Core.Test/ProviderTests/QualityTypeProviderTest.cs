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
        public void SetupDefault_should_add_all_profiles()
        {
            
            var db = TestDbHelper.GetEmptyDatabase();
            Mocker.SetConstant(db);

            //Act
            Mocker.Resolve<QualityTypeProvider>().SetupDefault();

            //Assert
            var types = Mocker.Resolve<QualityTypeProvider>().All();

            types.Should().HaveCount(7);
            types.Should().Contain(e => e.Name == "SDTV" && e.QualityTypeId == 1);
            types.Should().Contain(e => e.Name == "DVD" && e.QualityTypeId == 2);
            types.Should().Contain(e => e.Name == "HDTV" && e.QualityTypeId == 4);
            types.Should().Contain(e => e.Name == "WEBDL-720p" && e.QualityTypeId == 5);
            types.Should().Contain(e => e.Name == "WEBDL-1080p" && e.QualityTypeId == 3);
            types.Should().Contain(e => e.Name == "Bluray720p" && e.QualityTypeId == 6);
            types.Should().Contain(e => e.Name == "Bluray1080p" && e.QualityTypeId == 7);
        }

        [Test]
        public void SetupDefault_already_exists_should_insert_missing()
        {
            
            var db = TestDbHelper.GetEmptyDatabase();
            Mocker.SetConstant(db);

            db.Insert(new QualityType { QualityTypeId = 1, Name = "SDTV", MinSize = 0, MaxSize = 100 });

            //Act
            Mocker.Resolve<QualityTypeProvider>().SetupDefault();

            //Assert
            var types = Mocker.Resolve<QualityTypeProvider>().All();

            types.Should().HaveCount(7);
        }

        [Test]
        public void GetList_single_quality_type()
        {
            
            var db = TestDbHelper.GetEmptyDatabase();
            Mocker.SetConstant(db);

            var fakeQualityTypes = Builder<QualityType>.CreateListOfSize(6)
                .Build();

            var ids = new List<int> { 1 };

            db.InsertMany(fakeQualityTypes);

            //Act
            var result = Mocker.Resolve<QualityTypeProvider>().GetList(ids);

            //Assert
            result.Should().HaveCount(ids.Count);
        }

        [Test]
        public void GetList_multiple_quality_type()
        {
            
            var db = TestDbHelper.GetEmptyDatabase();
            Mocker.SetConstant(db);

            var fakeQualityTypes = Builder<QualityType>.CreateListOfSize(6)
                .Build();

            var ids = new List<int> { 1, 2 };

            db.InsertMany(fakeQualityTypes);

            //Act
            var result = Mocker.Resolve<QualityTypeProvider>().GetList(ids);

            //Assert
            result.Should().HaveCount(ids.Count);
        }
    }
}