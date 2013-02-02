using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class NewznabProviderTest : SqlCeTest<NewznabProvider>
    {
        [SetUp]
        public void SetUp()
        {
            WithRealDb();
            InitiateSubject();
        }


        [Test]
        public void Save_should_clean_url_before_inserting()
        {

            var newznab = new NewznabDefinition { Name = "Newznab Provider", Enable = true, Url = "http://www.nzbdrone.com/gibberish/test.aspx?hello=world" };
            const string expectedUrl = "http://www.nzbdrone.com";


            var result = Subject.Save(newznab);

            Db.Single<NewznabDefinition>(result).Url.Should().Be(expectedUrl);
        }

        [Test]
        public void Save_should_clean_url_before_updating()
        {
            var newznab = new NewznabDefinition { Name = "Newznab Provider", Enable = true, Url = "http://www.nzbdrone.com/gibberish/test.aspx?hello=world" };
            
            var result = Subject.Save(newznab);

            Subject.All().Single(c => c.Id == result).Url.Should().Be("http://www.nzbdrone.com");
        }

        [Test]
        public void Save_should_clean_url_before_inserting_when_url_is_not_empty()
        {
            var newznab = new NewznabDefinition { Name = "Newznab Provider", Enable = true, Url = "" };

            var result = Subject.Save(newznab);


            Subject.All().Single(c => c.Id == result).Url.Should().Be("");
        }

        [Test]
        public void Save_should_clean_url_before_updating_when_url_is_not_empty()
        {
            var newznab = new NewznabDefinition { Name = "Newznab Provider", Enable = true, Url = "" };
            var result = Subject.Save(newznab);
            
            Db.Single<NewznabDefinition>(result).Url.Should().Be("");
        }

        [Test]
        [Ignore("No longer clean newznab URLs")]
        public void SaveAll_should_clean_urls_before_updating()
        {

            var definitions = Builder<NewznabDefinition>.CreateListOfSize(5)
                .All()
                .With(d => d.Url = "http://www.nzbdrone.com")
                .Build();
            var expectedUrl = "http://www.nzbdrone.com";
            var newUrl = "http://www.nzbdrone.com/gibberish/test.aspx?hello=world";


            Db.InsertMany(definitions);

            definitions.ToList().ForEach(d => d.Url = newUrl);


            Subject.SaveAll(definitions);


            Db.Fetch<NewznabDefinition>().Where(d => d.Url == expectedUrl).Should().HaveCount(5);
        }

        [Test]
        public void Enabled_should_return_all_enabled_newznab_providers()
        {

            var definitions = Builder<NewznabDefinition>.CreateListOfSize(5)
                .TheFirst(2)
                .With(d => d.Enable = false)
                .TheLast(3)
                .With(d => d.Enable = true)
                .Build();


            Db.InsertMany(definitions);


            var result = Subject.Enabled();


            result.Should().HaveCount(3);
            result.All(d => d.Enable).Should().BeTrue();
        }

        [Test]
        public void All_should_return_all_newznab_providers()
        {

            var userIndexers = Builder<NewznabDefinition>.CreateListOfSize(5)
                .All().With(c => c.Url = "http://www.host.com/12")
                .Build();


            Db.InsertMany(userIndexers);

            var result = Subject.All();

            result.Should().HaveCount(8);
        }


        [Test]
        public void All_should_return_empty_list_when_no_indexers_exist()
        {
            Db.Delete<NewznabDefinition>("");

            Subject.All().Should().NotBeNull();
            Subject.All().Should().BeEmpty();

        }

        [Test]
        public void Delete_should_delete_newznab_provider()
        {
            var toBeDelete = Subject.All()[2];

            Subject.Delete(toBeDelete.Id);

            Subject.All().Should().NotBeEmpty();
            Subject.All().Should().NotContain(c => c.Id == toBeDelete.Id);
        }

        [Test]
        public void InitializeNewznabIndexers_should_initialize_build_in_indexers()
        {
            var indexers = Subject.All();

            indexers.Should().NotBeEmpty();
            indexers.Should().OnlyContain(i => i.BuiltIn);
        }

        [Test]
        public void InitializeNewznabIndexers_should_initialize_new_indexers_only()
        {

            var definitions = Builder<NewznabDefinition>.CreateListOfSize(5)
                .All()
                .With(d => d.Id = 0)
                .TheFirst(2)
                .With(d => d.Url = "http://www.nzbdrone2.com")
                .TheLast(3)
                .With(d => d.Url = "http://www.nzbdrone.com")
                .Build();


            Db.Insert(definitions[0]);
            Db.Insert(definitions[2]);

            Mocker.SetConstant<IEnumerable<NewznabDefinition>>(definitions);


            var result = Db.Fetch<NewznabDefinition>();
            result.Where(d => d.Url == "http://www.nzbdrone.com").Should().HaveCount(1);
            result.Where(d => d.Url == "http://www.nzbdrone2.com").Should().HaveCount(1);
        }

        [Test]
        public void InitializeNewznabIndexers_should_not_blow_up_if_more_than_one_indexer_with_the_same_url_is_found()
        {
            var definition = Builder<NewznabDefinition>.CreateNew()
                .With(d => d.Url = "http://www.nzbdrone2.com")
                .With(d => d.BuiltIn = false)
                .Build();

            Db.Insert(definition);
            Db.Insert(definition);


            Mocker.SetConstant<IEnumerable<NewznabDefinition>>(new List<NewznabDefinition> { definition });

            var result = Db.Fetch<NewznabDefinition>().Where(c => c.BuiltIn == false);
            result.Should().HaveCount(2);
        }

        [Test]
        public void CheckHostname_should_do_nothing_if_hostname_is_valid()
        {
            Subject.CheckHostname("http://www.google.com");
        }

        [Test]
        public void CheckHostname_should_log_error_and_throw_exception_if_dnsHostname_is_invalid()
        {
            Assert.Throws<SocketException>(() => Subject.CheckHostname("http://BadName"));

            ExceptionVerification.ExpectedErrors(1);
        }
    }
}