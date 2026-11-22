using System;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Cache;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Authentication
{
    [TestFixture]
    public class OidcLogoutServiceFixture : DbTest<OidcLogoutService, OidcLogout>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.SetConstant<IOidcLogoutRepository>(Mocker.Resolve<OidcLogoutRepository>());
            Mocker.SetConstant<ICacheManager>(Mocker.Resolve<CacheManager>());
        }

        private OidcLogoutService GivenRestartedService()
        {
            return new OidcLogoutService(Mocker.Resolve<IOidcLogoutRepository>(), new CacheManager());
        }

        [Test]
        public void should_persist_session_logout()
        {
            Subject.LogoutSession("session1");

            AllStoredModels.Should().ContainSingle(l => l.SessionId == "session1" && l.Subject == null);
        }

        [Test]
        public void should_log_out_matching_session()
        {
            Subject.LogoutSession("session1");

            Subject.IsLoggedOut("session1", "subject1", DateTime.UtcNow.AddMinutes(-5)).Should().BeTrue();
            Subject.IsLoggedOut("session2", "subject1", DateTime.UtcNow.AddMinutes(-5)).Should().BeFalse();
        }

        [Test]
        public void should_log_out_sessions_for_subject_issued_before_logout()
        {
            Subject.LogoutSubject("subject1");

            Subject.IsLoggedOut("session1", "subject1", DateTime.UtcNow.AddMinutes(-5)).Should().BeTrue();
            Subject.IsLoggedOut(null, "subject1", null).Should().BeTrue();
            Subject.IsLoggedOut("session1", "subject1", DateTime.UtcNow.AddMinutes(5)).Should().BeFalse();
            Subject.IsLoggedOut("session1", "subject2", DateTime.UtcNow.AddMinutes(-5)).Should().BeFalse();
        }

        [Test]
        public void should_see_logout_made_after_logouts_were_cached()
        {
            Subject.IsLoggedOut("session1", "subject1", null).Should().BeFalse();

            Subject.LogoutSession("session1");

            Subject.IsLoggedOut("session1", "subject1", null).Should().BeTrue();
        }

        [Test]
        public void should_keep_logouts_after_restart()
        {
            Subject.LogoutSession("session1");
            Subject.LogoutSubject("subject2");

            var restarted = GivenRestartedService();

            restarted.IsLoggedOut("session1", "subject1", DateTime.UtcNow.AddMinutes(-5)).Should().BeTrue();
            restarted.IsLoggedOut("session3", "subject2", DateTime.UtcNow.AddMinutes(-5)).Should().BeTrue();
        }
    }
}
