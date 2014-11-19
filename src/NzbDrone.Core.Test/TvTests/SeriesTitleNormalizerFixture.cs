using System;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.TvTests
{
    [TestFixture]
    public class SeriesTitleNormalizerFixture
    {
        [Test]
        public void should_use_precomputed_title_for_a_to_z()
        {
            SeriesTitleNormalizer.Normalize("A to Z", 281588).Should().Be("a to z");
        }

        [TestCase("2 Broke Girls", "2 broke girls")]
        [TestCase("Archer (2009)", "archer 2009")]
        [TestCase("The Office (US)", "office us")]
        [TestCase("The Mentalist", "mentalist")]
        [TestCase("The Good Wife", "good wife")]
        [TestCase("The Newsroom (2012)", "newsroom 2012")]
        [TestCase("Special Agent Oso", "special agent oso")]
        public void should_normalize_title(String title, String expected)
        {
            SeriesTitleNormalizer.Normalize(title, 0).Should().Be(expected);
        }
    }
}
