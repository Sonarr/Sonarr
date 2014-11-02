using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Indexers.NzbIndex;

namespace NzbDrone.Core.Test.IndexerTests.NzbIndexTests
{
    public class NzbIndexSettingFixture
    {
        [TestCase("http:///www.nzbindex.com")]
        public void requires_validate_all_fields(string url)
        {
            var setting = new NzbIndexSettings()
            {
                QueryParam = String.Empty,
                AdditionalParameters = String.Empty,
                MaxAgeParam = String.Empty,
                ResponseMaxSize = -1,
                MaxSizeParam = String.Empty,
                MinSizeParam = String.Empty,
                ResponseMaxSizeParam = String.Empty,
                Url = url
            };


            setting.Validate().IsValid.Should().BeFalse();
            setting.Validate().Errors.Should().Contain(c => c.PropertyName == "QueryParam");
            setting.Validate().Errors.Should().NotContain(c => c.PropertyName == "AdditionalParameters");
            setting.Validate().Errors.Should().Contain(c => c.PropertyName == "MaxAgeParam");
            setting.Validate().Errors.Should().Contain(c => c.PropertyName == "ResponseMaxSize");
            setting.Validate().Errors.Should().Contain(c => c.PropertyName == "MaxSizeParam");
            setting.Validate().Errors.Should().Contain(c => c.PropertyName == "MinSizeParam");
            setting.Validate().Errors.Should().Contain(c => c.PropertyName == "ResponseMaxSizeParam");

        }
    }
}
