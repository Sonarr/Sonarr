using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Policy;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Indexers
{
    [TestFixture]
    
    public class NewznabProviderTest : CoreTest<NewznabService>
    {
        private void WithInvalidName()
        {
            Mocker.GetMock<INewznabRepository>()
                  .Setup(s => s.All())
                  .Returns(new List<NewznabDefinition>{new NewznabDefinition { Id = 1, Name = "", Url = "http://www.nzbdrone.com" }});
        }

        private void WithExisting()
        {
            Mocker.GetMock<INewznabRepository>()
                  .Setup(s => s.All())
                  .Returns(new List<NewznabDefinition> { new NewznabDefinition { Id = 1, Name = "Nzbs.org", Url = "http://nzbs.org" } });
        }

        [Test]
        public void InitializeNewznabIndexers_should_initialize_build_in_indexers()
        {
            Subject.Init();

            Mocker.GetMock<INewznabRepository>()
                  .Verify(s => s.Insert(It.Is<NewznabDefinition>(n => n.BuiltIn)), Times.Exactly(3));
        }

        [Test]
        public void should_delete_indexers_without_names()
        {
            WithInvalidName();

            Subject.Init();

            Mocker.GetMock<INewznabRepository>()
                  .Verify(s => s.Delete(1), Times.Once());
        }

        [Test]
        public void should_add_new_indexers()
        {
            WithExisting();
            
            Subject.Init();

            Mocker.GetMock<INewznabRepository>()
                  .Verify(s => s.Insert(It.IsAny<NewznabDefinition>()), Times.Exactly(2));
        }

        [Test]
        public void should_update_existing()
        {
            WithExisting();

            Subject.Init();

            Mocker.GetMock<INewznabRepository>()
                  .Verify(s => s.Update(It.IsAny<NewznabDefinition>()), Times.Once());
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