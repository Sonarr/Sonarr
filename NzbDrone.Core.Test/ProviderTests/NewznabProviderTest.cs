using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.ProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class NewznabProviderTest : CoreTest
    {
        [Test]
        public void Save_should_clean_url_before_inserting()
        {
            //Setup
            var newznab = new NewznabDefinition { Name = "Newznab Provider", Enable = true, Url = "http://www.nzbdrone.com/gibberish/test.aspx?hello=world" };
            var expectedUrl = "http://www.nzbdrone.com";

            
            var db = TestDbHelper.GetEmptyDatabase();
            Mocker.SetConstant(db);

            //Act
            var result = Mocker.Resolve<NewznabProvider>().Save(newznab);

            //Assert
            db.Single<NewznabDefinition>(result).Url.Should().Be(expectedUrl);
        }

        [Test]
        public void Save_should_clean_url_before_updating()
        {
            //Setup
            var newznab = new NewznabDefinition { Name = "Newznab Provider", Enable = true, Url = "http://www.nzbdrone.com" };
            var expectedUrl = "http://www.nzbdrone.com";
            var newUrl = "http://www.nzbdrone.com/gibberish/test.aspx?hello=world";

            
            var db = TestDbHelper.GetEmptyDatabase();
            Mocker.SetConstant(db);

            newznab.Id = Convert.ToInt32(db.Insert(newznab));
            newznab.Url = newUrl;

            //Act
            var result = Mocker.Resolve<NewznabProvider>().Save(newznab);

            //Assert
            db.Single<NewznabDefinition>(result).Url.Should().Be(expectedUrl);
        }

        [Test]
        public void Save_should_clean_url_before_inserting_when_url_is_not_empty()
        {
            //Setup
            var newznab = new NewznabDefinition { Name = "Newznab Provider", Enable = true, Url = "" };
            var expectedUrl = "";

            
            var db = TestDbHelper.GetEmptyDatabase();
            Mocker.SetConstant(db);

            //Act
            var result = Mocker.Resolve<NewznabProvider>().Save(newznab);

            //Assert
            db.Single<NewznabDefinition>(result).Url.Should().Be(expectedUrl);
        }

        [Test]
        public void Save_should_clean_url_before_updating_when_url_is_not_empty()
        {
            //Setup
            var newznab = new NewznabDefinition { Name = "Newznab Provider", Enable = true, Url = "http://www.nzbdrone.com" };
            var expectedUrl = "";
            var newUrl = "";

            
            var db = TestDbHelper.GetEmptyDatabase();
            Mocker.SetConstant(db);

            newznab.Id = Convert.ToInt32(db.Insert(newznab));
            newznab.Url = newUrl;

            //Act
            var result = Mocker.Resolve<NewznabProvider>().Save(newznab);

            //Assert
            db.Single<NewznabDefinition>(result).Url.Should().Be(expectedUrl);
        }

        [Test]
        public void SaveAll_should_clean_urls_before_updating()
        {
            //Setup
            var definitions = Builder<NewznabDefinition>.CreateListOfSize(5)
                .All()
                .With(d => d.Url = "http://www.nzbdrone.com")
                .Build();
            var expectedUrl = "http://www.nzbdrone.com";
            var newUrl = "http://www.nzbdrone.com/gibberish/test.aspx?hello=world";

            
            var db = TestDbHelper.GetEmptyDatabase();
            Mocker.SetConstant(db);

            db.InsertMany(definitions);

            definitions.ToList().ForEach(d => d.Url = newUrl);

            //Act
            Mocker.Resolve<NewznabProvider>().SaveAll(definitions);

            //Assert
            db.Fetch<NewznabDefinition>().Where(d => d.Url == expectedUrl).Should().HaveCount(5);
        }

        [Test]
        public void Enabled_should_return_all_enabled_newznab_providers()
        {
            //Setup
            var definitions = Builder<NewznabDefinition>.CreateListOfSize(5)
                .TheFirst(2)
                .With(d => d.Enable = false)
                .TheLast(3)
                .With(d => d.Enable = true)
                .Build();

            
            var db = TestDbHelper.GetEmptyDatabase();
            Mocker.SetConstant(db);

            db.InsertMany(definitions);

            //Act
            var result = Mocker.Resolve<NewznabProvider>().Enabled();

            //Assert
            result.Should().HaveCount(3);
            result.All(d => d.Enable).Should().BeTrue();
        }

        [Test]
        public void All_should_return_all_newznab_providers()
        {
            //Setup
            var definitions = Builder<NewznabDefinition>.CreateListOfSize(5)
                .TheFirst(2)
                .With(d => d.Enable = false)
                .TheLast(3)
                .With(d => d.Enable = true)
                .Build();

            
            var db = TestDbHelper.GetEmptyDatabase();
            Mocker.SetConstant(db);

            db.InsertMany(definitions);

            //Act
            var result = Mocker.Resolve<NewznabProvider>().All();

            //Assert
            result.Should().HaveCount(5);
        }

        [Test]
        public void Delete_should_delete_newznab_provider()
        {
            //Setup
            var definitions = Builder<NewznabDefinition>.CreateListOfSize(5)
                .TheFirst(2)
                .With(d => d.Enable = false)
                .TheLast(3)
                .With(d => d.Enable = true)
                .Build();

            
            var db = TestDbHelper.GetEmptyDatabase();
            Mocker.SetConstant(db);

            db.InsertMany(definitions);

            //Act
            Mocker.Resolve<NewznabProvider>().Delete(1);

            //Assert
            var result = db.Fetch<NewznabDefinition>();
            result.Should().HaveCount(4);
            result.Any(d => d.Id == 1).Should().BeFalse();
        }

        [Test]
        public void InitializeNewznabIndexers_should_initialize_new_indexers()
        {
            //Setup
            var definitions = Builder<NewznabDefinition>.CreateListOfSize(5)
                .All()
                .With(d => d.Url = "http://www.nzbdrone.com")
                .Build();

            
            var db = TestDbHelper.GetEmptyDatabase();
            Mocker.SetConstant(db);

            //Act
            Mocker.Resolve<NewznabProvider>().InitializeNewznabIndexers(definitions);

            //Assert
            var result = db.Fetch<NewznabDefinition>();
            result.Should().HaveCount(5);
            result.Should().OnlyContain(i => i.BuiltIn);
        }

        [Test]
        public void InitializeNewznabIndexers_should_initialize_new_indexers_only()
        {
            //Setup
            var definitions = Builder<NewznabDefinition>.CreateListOfSize(5)
                .All()
                .With(d => d.Id = 0)
                .TheFirst(2)
                .With(d => d.Url = "http://www.nzbdrone2.com")
                .TheLast(3)
                .With(d => d.Url = "http://www.nzbdrone.com")
                .Build();

            
            var db = TestDbHelper.GetEmptyDatabase();
            Mocker.SetConstant(db);

            db.Insert(definitions[0]);
            db.Insert(definitions[2]);

            //Act
            Mocker.Resolve<NewznabProvider>().InitializeNewznabIndexers(definitions);

            //Assert
            var result = db.Fetch<NewznabDefinition>();
            result.Should().HaveCount(2);
            result.Where(d => d.Url == "http://www.nzbdrone.com").Should().HaveCount(1);
            result.Where(d => d.Url == "http://www.nzbdrone2.com").Should().HaveCount(1);
        }

        [Test]
        public void InitializeNewznabIndexers_should_update_matching_indexer_to_be_builtin()
        {
            //Setup
            var definition = Builder<NewznabDefinition>.CreateNew()
                .With(d => d.Url = "http://www.nzbdrone2.com")
                .With(d => d.BuiltIn = false)
                .Build();

            var db = TestDbHelper.GetEmptyDatabase();
            Mocker.SetConstant(db);

            db.Insert(definition);

            //Act
            Mocker.Resolve<NewznabProvider>().InitializeNewznabIndexers(new List<NewznabDefinition>{ definition });

            //Assert
            var result = db.Fetch<NewznabDefinition>();
            result.Should().HaveCount(1);
            result.First().BuiltIn.Should().BeTrue();
        }
    }
}