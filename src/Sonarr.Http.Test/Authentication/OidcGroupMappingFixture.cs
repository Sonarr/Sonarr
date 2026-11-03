using System.Collections.Generic;

using NUnit.Framework;

using NzbDrone.Core.Authentication;
using Sonarr.Http.Authentication;

namespace Sonarr.Http.Test.Authentication
{
    [TestFixture]
    public class OidcGroupMappingFixture
    {
        [Test]
        public void should_map_homies_to_readonly()
        {
            Assert.That(OidcGroupMapping.Resolve(new[] { "homies" }), Is.EqualTo(SonarrRole.ReadOnly));
        }

        [Test]
        public void should_map_family_to_addsearch()
        {
            Assert.That(OidcGroupMapping.Resolve(new[] { "family" }), Is.EqualTo(SonarrRole.AddSearch));
        }

        [Test]
        public void should_map_maintenance_to_manage()
        {
            Assert.That(OidcGroupMapping.Resolve(new[] { "maintenance" }), Is.EqualTo(SonarrRole.Manage));
        }

        [Test]
        public void should_map_authentik_admins_to_admin()
        {
            Assert.That(OidcGroupMapping.Resolve(new[] { "authentik Admins" }), Is.EqualTo(SonarrRole.Admin));
        }

        [Test]
        public void should_return_null_for_pirates_only()
        {
            Assert.That(OidcGroupMapping.Resolve(new[] { "pirates" }), Is.Null);
        }

        [Test]
        public void should_return_null_for_empty()
        {
            Assert.That(OidcGroupMapping.Resolve(new string[0]), Is.Null);
            Assert.That(OidcGroupMapping.Resolve((IEnumerable<string>)null), Is.Null);
        }

        [Test]
        public void should_be_case_insensitive()
        {
            Assert.That(OidcGroupMapping.Resolve(new[] { "HOMIES" }), Is.EqualTo(SonarrRole.ReadOnly));
            Assert.That(OidcGroupMapping.Resolve(new[] { "Authentik ADMINS" }), Is.EqualTo(SonarrRole.Admin));
        }

        [Test]
        public void should_ignore_unknown_groups()
        {
            Assert.That(OidcGroupMapping.Resolve(new[] { "foo", "bar" }), Is.Null);
            Assert.That(OidcGroupMapping.Resolve(new[] { "foo", "homies" }), Is.EqualTo(SonarrRole.ReadOnly));
        }

        [Test]
        public void should_pick_highest_tier_when_in_multiple_groups()
        {
            Assert.That(OidcGroupMapping.Resolve(new[] { "homies", "maintenance" }), Is.EqualTo(SonarrRole.Manage));
            Assert.That(OidcGroupMapping.Resolve(new[] { "family", "authentik Admins" }), Is.EqualTo(SonarrRole.Admin));
            Assert.That(OidcGroupMapping.Resolve(new[] { "pirates", "homies" }), Is.EqualTo(SonarrRole.ReadOnly));
        }

        [Test]
        public void should_have_five_entries_in_group_to_tier()
        {
            Assert.That(OidcGroupMapping.GroupToTier.Count, Is.EqualTo(5));
        }

        [Test]
        public void should_return_null_for_pirates_plus_unknown()
        {
            Assert.That(OidcGroupMapping.Resolve(new[] { "pirates", "unknown" }), Is.Null);
        }

        [Test]
        public void should_pick_highest_tier_with_mixed_unknown_groups()
        {
            Assert.That(OidcGroupMapping.Resolve(new[] { "foo", "family", "bar", "maintenance" }), Is.EqualTo(SonarrRole.Manage));
            Assert.That(OidcGroupMapping.Resolve(new[] { "homies", "baz", "authentik Admins" }), Is.EqualTo(SonarrRole.Admin));
        }

        [Test]
        public void should_be_case_insensitive_for_all_tiers()
        {
            Assert.That(OidcGroupMapping.Resolve(new[] { "homies", "FAMILY", "Maintenance", "AUTHENTIK ADMINS" }), Is.EqualTo(SonarrRole.Admin));
        }

        [Test]
        public void should_resolve_exact_match_with_mappings()
        {
            Assert.That(OidcGroupMapping.Resolve(new[] { "homies" }, "homies=ReadOnly"), Is.EqualTo(SonarrRole.ReadOnly));
            Assert.That(OidcGroupMapping.Resolve(new[] { "family" }, "family=AddSearch"), Is.EqualTo(SonarrRole.AddSearch));
        }

        [Test]
        public void should_be_case_insensitive_for_group_and_role_with_mappings()
        {
            Assert.That(OidcGroupMapping.Resolve(new[] { "HOMIES" }, "homies=readonly"), Is.EqualTo(SonarrRole.ReadOnly));
            Assert.That(OidcGroupMapping.Resolve(new[] { "Homies" }, "HOMIES=READONLY"), Is.EqualTo(SonarrRole.ReadOnly));
        }

        [Test]
        public void should_pick_highest_role_with_mappings()
        {
            Assert.That(OidcGroupMapping.Resolve(new[] { "homies", "maintenance" }, "homies=ReadOnly;maintenance=Manage"), Is.EqualTo(SonarrRole.Manage));
        }

        [Test]
        public void should_return_null_for_unmapped_group_with_mappings()
        {
            Assert.That(OidcGroupMapping.Resolve(new[] { "foo" }, "homies=ReadOnly"), Is.Null);
        }

        [Test]
        public void should_return_null_for_empty_mappings()
        {
            Assert.That(OidcGroupMapping.Resolve(new[] { "homies" }, ""), Is.Null);
            Assert.That(OidcGroupMapping.Resolve(new[] { "homies" }, "   "), Is.Null);
        }

        [Test]
        public void should_ignore_malformed_pairs()
        {
            Assert.That(OidcGroupMapping.Resolve(new[] { "family" }, "noequals;family=AddSearch"), Is.EqualTo(SonarrRole.AddSearch));
            Assert.That(OidcGroupMapping.Resolve(new[] { "homies" }, "homies=;family=AddSearch"), Is.Null);
            Assert.That(OidcGroupMapping.Resolve(new[] { "family" }, "=ReadOnly;family=AddSearch"), Is.EqualTo(SonarrRole.AddSearch));
            Assert.That(OidcGroupMapping.Resolve(new[] { "homies" }, "homies=SuperUser"), Is.Null);
        }
    }
}
