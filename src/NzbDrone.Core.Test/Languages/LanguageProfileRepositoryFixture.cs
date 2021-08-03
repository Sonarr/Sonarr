using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Profiles.Languages;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Languages
{
    [TestFixture]
    public class LanguageProfileRepositoryFixture : DbTest<LanguageProfileRepository, LanguageProfile>
    {
        [Test]
        public void should_be_able_to_read_and_write()
        {
            var profile = new LanguageProfile
                {
                    Languages = Language.All.OrderByDescending(l => l.Name).Select(l => new LanguageProfileItem { Language = l, Allowed = l == Language.English }).ToList(),
                    Name = "TestProfile",
                    Cutoff = Language.English
                };

            Subject.Insert(profile);

            StoredModel.Name.Should().Be(profile.Name);
            StoredModel.Cutoff.Should().Be(profile.Cutoff);

            StoredModel.Languages.Should().Equal(profile.Languages, (a, b) => a.Language == b.Language && a.Allowed == b.Allowed);
        }
    }
}
