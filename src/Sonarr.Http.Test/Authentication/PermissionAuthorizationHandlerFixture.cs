using System.Security.Claims;

using FluentAssertions;

using Microsoft.AspNetCore.Authorization;

using NUnit.Framework;

using NzbDrone.Core.Authentication;

using Sonarr.Http.Authentication;

namespace Sonarr.Http.Test.Authentication
{
    [TestFixture]
    public class PermissionAuthorizationHandlerFixture
    {
        private readonly PermissionAuthorizationHandler _handler = new PermissionAuthorizationHandler();

        private bool IsAuthorized(ClaimsPrincipal principal, SonarrRole minimum)
        {
            var requirement = new PermissionRequirement(minimum);
            var context = new AuthorizationHandlerContext(new[] { requirement }, principal, null);

            _handler.HandleAsync(context).GetAwaiter().GetResult();

            return context.HasSucceeded;
        }

        private static ClaimsPrincipal PrincipalWithRole(string role)
        {
            var identity = new ClaimsIdentity(new[] { new Claim("role", role) }, "test");
            return new ClaimsPrincipal(identity);
        }

        [Test]
        public void admin_succeeds_for_readonly_requirement()
        {
            IsAuthorized(PrincipalWithRole("Admin"), SonarrRole.ReadOnly).Should().BeTrue();
        }

        [Test]
        public void readonly_fails_for_manage_requirement()
        {
            IsAuthorized(PrincipalWithRole("ReadOnly"), SonarrRole.Manage).Should().BeFalse();
        }

        [Test]
        public void no_role_no_oidc_sub_bypasses()
        {
            var identity = new ClaimsIdentity(new[] { new Claim("ApiKey", "true") }, "API Key");
            var principal = new ClaimsPrincipal(identity);

            IsAuthorized(principal, SonarrRole.Manage).Should().BeTrue();
        }

        [Test]
        public void unauthenticated_principal_does_not_bypass()
        {
            var principal = new ClaimsPrincipal(new ClaimsIdentity());

            IsAuthorized(principal, SonarrRole.Manage).Should().BeFalse();
            IsAuthorized(principal, SonarrRole.ReadOnly).Should().BeFalse();
        }

        [Test]
        public void oidc_sub_without_role_denies()
        {
            var identity = new ClaimsIdentity(new[] { new Claim("oidc_sub", "subject-1") }, "Cookies");
            var principal = new ClaimsPrincipal(identity);

            IsAuthorized(principal, SonarrRole.Admin).Should().BeFalse();
        }

        [Test]
        public void oidc_sub_with_unparseable_sonarr_role_denies()
        {
            var identity = new ClaimsIdentity(new[] { new Claim("oidc_sub", "subject-1"), new Claim("sonarr_role", "garbage") }, "Cookies");
            var principal = new ClaimsPrincipal(identity);

            IsAuthorized(principal, SonarrRole.Admin).Should().BeFalse();
        }

        [Test]
        public void unparseable_role_defaults_to_admin()
        {
            var identity = new ClaimsIdentity(new[] { new Claim("role", "garbage") }, "test");
            var principal = new ClaimsPrincipal(identity);

            IsAuthorized(principal, SonarrRole.Admin).Should().BeTrue();
        }

        [Test]
        public void readonly_succeeds_for_readonly_requirement()
        {
            IsAuthorized(PrincipalWithRole("ReadOnly"), SonarrRole.ReadOnly).Should().BeTrue();
        }

        [Test]
        public void readonly_fails_for_addsearch_requirement()
        {
            IsAuthorized(PrincipalWithRole("ReadOnly"), SonarrRole.AddSearch).Should().BeFalse();
        }

        [Test]
        public void addsearch_succeeds_for_readonly_requirement()
        {
            IsAuthorized(PrincipalWithRole("AddSearch"), SonarrRole.ReadOnly).Should().BeTrue();
        }

        [Test]
        public void addsearch_succeeds_for_addsearch_requirement()
        {
            IsAuthorized(PrincipalWithRole("AddSearch"), SonarrRole.AddSearch).Should().BeTrue();
        }

        [Test]
        public void addsearch_fails_for_manage_requirement()
        {
            IsAuthorized(PrincipalWithRole("AddSearch"), SonarrRole.Manage).Should().BeFalse();
        }

        [Test]
        public void manage_succeeds_for_manage_requirement()
        {
            IsAuthorized(PrincipalWithRole("Manage"), SonarrRole.Manage).Should().BeTrue();
        }

        [Test]
        public void manage_fails_for_admin_requirement()
        {
            IsAuthorized(PrincipalWithRole("Manage"), SonarrRole.Admin).Should().BeFalse();
        }

        [Test]
        public void admin_succeeds_for_all_requirements()
        {
            IsAuthorized(PrincipalWithRole("Admin"), SonarrRole.ReadOnly).Should().BeTrue();
            IsAuthorized(PrincipalWithRole("Admin"), SonarrRole.AddSearch).Should().BeTrue();
            IsAuthorized(PrincipalWithRole("Admin"), SonarrRole.Manage).Should().BeTrue();
            IsAuthorized(PrincipalWithRole("Admin"), SonarrRole.Admin).Should().BeTrue();
        }

        [Test]
        public void api_key_without_role_or_oidc_sub_bypasses_all_requirements()
        {
            var identity = new ClaimsIdentity(new[] { new Claim("ApiKey", "true") }, "API Key");
            var principal = new ClaimsPrincipal(identity);

            IsAuthorized(principal, SonarrRole.ReadOnly).Should().BeTrue();
            IsAuthorized(principal, SonarrRole.AddSearch).Should().BeTrue();
            IsAuthorized(principal, SonarrRole.Manage).Should().BeTrue();
            IsAuthorized(principal, SonarrRole.Admin).Should().BeTrue();
        }

        [Test]
        public void role_claim_is_case_insensitive()
        {
            IsAuthorized(PrincipalWithRole("admin"), SonarrRole.Admin).Should().BeTrue();
            IsAuthorized(PrincipalWithRole("readonly"), SonarrRole.ReadOnly).Should().BeTrue();
        }
    }
}
