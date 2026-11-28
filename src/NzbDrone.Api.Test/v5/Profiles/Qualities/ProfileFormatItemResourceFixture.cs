using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Profiles;
using Sonarr.Api.V5.Profiles.Quality;

namespace NzbDrone.Api.Test.v5.Profiles.Qualities
{
    [TestFixture]
    public class ProfileFormatItemResourceFixture
    {
        [Test]
        public void should_use_display_name_when_set()
        {
            var item = new ProfileFormatItem
            {
                Format = new CustomFormat { Id = 1, Name = "vietnamese", DisplayName = "Language: Vietnamese" },
                Score = 50
            };

            var resource = item.ToResource();

            resource.Name.Should().Be("Language: Vietnamese");
        }

        [TestCase("")]
        [TestCase(null)]
        public void should_fall_back_to_name_when_display_name_is_empty(string displayName)
        {
            var item = new ProfileFormatItem
            {
                Format = new CustomFormat { Id = 1, Name = "vietnamese", DisplayName = displayName },
                Score = 50
            };

            var resource = item.ToResource();

            resource.Name.Should().Be("vietnamese");
        }
    }
}
