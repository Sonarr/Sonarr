using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Serializer;
using Sonarr.Api.V3.Profiles.Release;
using Sonarr.Http.REST;

namespace NzbDrone.Api.Test.v3.ReleaseProfiles
{
    [TestFixture]
    public class ReleaseProfilesFixture
    {
        [Test]
        public void should_deserialize_releaseprofile_v3_ignored_null()
        {
            var resource = STJson.Deserialize<ReleaseProfileResource>("{ \"ignored\": null, \"required\": null }");

            var model = resource.ToModel();

            model.Ignored.Should().BeEquivalentTo();
            model.Required.Should().BeEquivalentTo();
        }

        [Test]
        public void should_deserialize_releaseprofile_v3_ignored_string()
        {
            var resource = STJson.Deserialize<ReleaseProfileResource>("{ \"ignored\": \"testa,testb\", \"required\": \"testc,testd\" }");

            var model = resource.ToModel();

            model.Ignored.Should().BeEquivalentTo("testa", "testb");
            model.Required.Should().BeEquivalentTo("testc", "testd");
        }

        [Test]
        public void should_deserialize_releaseprofile_v3_ignored_string_array()
        {
            var resource = STJson.Deserialize<ReleaseProfileResource>("{ \"ignored\": [ \"testa\", \"testb\" ], \"required\": [ \"testc\", \"testd\" ] }");

            var model = resource.ToModel();

            model.Ignored.Should().BeEquivalentTo("testa", "testb");
            model.Required.Should().BeEquivalentTo("testc", "testd");
        }

        [Test]
        public void should_throw_with_bad_releaseprofile_v3_ignored_type()
        {
            var resource = STJson.Deserialize<ReleaseProfileResource>("{ \"ignored\": {} }");

            Assert.Throws<BadRequestException>(() => resource.ToModel());
        }
    }
}
