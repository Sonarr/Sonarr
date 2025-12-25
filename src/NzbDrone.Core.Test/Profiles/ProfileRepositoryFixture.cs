using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Profiles
{
    [TestFixture]
    public class ProfileRepositoryFixture : DbTest<QualityProfileRepository, QualityProfile>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<ICustomFormatService>()
                  .Setup(s => s.All())
                  .Returns(new List<CustomFormat>());

            Mocker.GetMock<ICustomFormatService>()
                  .Setup(s => s.All())
                  .Returns(new List<CustomFormat>());
        }

        [Test]
        public async Task should_be_able_to_read_and_write()
        {
            var profile = new QualityProfile
            {
                Items = Qualities.QualityFixture.GetDefaultQualities(Quality.Bluray1080p, Quality.DVD, Quality.HDTV720p),
                Cutoff = Quality.Bluray1080p.Id,
                Name = "TestProfile"
            };

            await Subject.InsertAsync(profile);

            var enumerable = await Subject.AllAsync();
            var storedModel = enumerable.Single();
            storedModel.Name.Should().Be(profile.Name);
            storedModel.Cutoff.Should().Be(profile.Cutoff);

            storedModel.Items.Should().Equal(profile.Items, (a, b) => a.Quality == b.Quality && a.Allowed == b.Allowed);
        }
    }
}
