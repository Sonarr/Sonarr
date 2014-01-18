using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Qualities
{
    [TestFixture]
    public class QualityProfileRepositoryFixture : DbTest<QualityProfileRepository, QualityProfile>
    {
        [Test]
        public void should_be_able_to_read_and_write()
        {
            var profile = new QualityProfile
                {
                    Allowed = new List<Quality>
                        {
                            Quality.Bluray1080p,
                            Quality.DVD,
                            Quality.HDTV720p
                        },

                    Cutoff = Quality.Bluray1080p,
                    Name = "TestProfile"
                };

            Subject.Insert(profile);

            StoredModel.Name.Should().Be(profile.Name);
            StoredModel.Cutoff.Should().Be(profile.Cutoff);

            StoredModel.Allowed.Should().BeEquivalentTo(profile.Allowed);


        }
    }
}