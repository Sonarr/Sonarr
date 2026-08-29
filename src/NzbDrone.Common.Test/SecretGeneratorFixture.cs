using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test
{
    [TestFixture]
    public class SecretGeneratorFixture : TestBase
    {
        [TestCase(12)]
        [TestCase(20)]
        public void should_generate_a_secret_of_the_requested_length(int length)
        {
            SecretGenerator.Generate(length).Length.Should().Be(length);
        }

        [Test]
        public void should_not_contain_ambiguous_characters()
        {
            var secret = string.Join(string.Empty, Enumerable.Range(0, 100).Select(_ => SecretGenerator.Generate(20)));

            secret.Should().NotContain("0");
            secret.Should().NotContain("O");
            secret.Should().NotContain("1");
            secret.Should().NotContain("l");
            secret.Should().NotContain("I");
        }

        [Test]
        public void should_generate_a_different_secret_each_time()
        {
            var secrets = Enumerable.Range(0, 100).Select(_ => SecretGenerator.Generate(20)).ToList();

            secrets.Distinct().Should().HaveCount(secrets.Count);
        }
    }
}
