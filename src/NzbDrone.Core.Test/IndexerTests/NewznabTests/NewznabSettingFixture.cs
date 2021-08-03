using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Indexers.Newznab;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerTests.NewznabTests
{
    public class NewznabSettingFixture : CoreTest
    {
        [TestCase("http://nzbs.org")]
        [TestCase("http:///www.nzbplanet.net")]
        public void requires_apikey(string url)
        {
            var setting = new NewznabSettings()
            {
                ApiKey = "",
                BaseUrl = url
            };

            setting.Validate().IsValid.Should().BeFalse();
            setting.Validate().Errors.Should().Contain(c => c.PropertyName == nameof(NewznabSettings.ApiKey));
        }

        [TestCase("")]
        [TestCase("  ")]
        [TestCase(null)]
        public void invalid_url_should_not_apikey(string url)
        {
            var setting = new NewznabSettings
            {
                ApiKey = "",
                BaseUrl = url
            };

            setting.Validate().IsValid.Should().BeFalse();
            setting.Validate().Errors.Should().NotContain(c => c.PropertyName == nameof(NewznabSettings.ApiKey));
            setting.Validate().Errors.Should().Contain(c => c.PropertyName == nameof(NewznabSettings.BaseUrl));
        }

        [TestCase("http://nzbs2.org")]
        public void doesnt_requires_apikey(string url)
        {
            var setting = new NewznabSettings()
            {
                ApiKey = "",
                BaseUrl = url
            };

            setting.Validate().IsValid.Should().BeTrue();
        }
    }
}
