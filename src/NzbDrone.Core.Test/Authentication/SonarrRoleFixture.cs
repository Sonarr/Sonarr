using NUnit.Framework;
using NzbDrone.Core.Authentication;

namespace NzbDrone.Core.Test.Authentication
{
    [TestFixture]
    public class SonarrRoleFixture
    {
        [Test]
        public void should_have_expected_values()
        {
            Assert.That((int)SonarrRole.ReadOnly, Is.EqualTo(0));
            Assert.That((int)SonarrRole.AddSearch, Is.EqualTo(1));
            Assert.That((int)SonarrRole.Manage, Is.EqualTo(2));
            Assert.That((int)SonarrRole.Admin, Is.EqualTo(3));
        }

        [Test]
        public void should_order_readonly_less_than_addsearch_less_than_manage_less_than_admin()
        {
            Assert.That(SonarrRole.ReadOnly, Is.LessThan(SonarrRole.AddSearch));
            Assert.That(SonarrRole.AddSearch, Is.LessThan(SonarrRole.Manage));
            Assert.That(SonarrRole.Manage, Is.LessThan(SonarrRole.Admin));
        }
    }
}
