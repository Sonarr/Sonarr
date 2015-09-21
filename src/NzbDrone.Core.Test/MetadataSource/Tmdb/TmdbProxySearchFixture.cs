using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MetadataSource.Tmdb;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.Categories;

namespace NzbDrone.Core.Test.MetadataSource.Tmdb
{
    [TestFixture]
    [IntegrationTest]
    public class TmdbProxySearchFixture : CoreTest<TmdbProxy>
    {
        [SetUp]
        public void Setup()
        {
            UseRealHttp();
        }

        [TestCase("Spanish affair", "Spanish Affair")]
        [TestCase("Ocho apellidos vascos", "Spanish Affair")]
        [TestCase("House", "House")]
        [TestCase("Zipper", "Zipper")]
        [TestCase("One Flew Over the Cuckoo's Nest", "One Flew Over the Cuckoo's Nest")]
        [TestCase("tmdbid: 289578", "Meine Kinder und ich")]
        [TestCase("tmdb:289578", "Meine Kinder und ich")]
        [TestCase("tmdb: 289578", "Meine Kinder und ich")]
        [TestCase("tmdbid:289578", "Meine Kinder und ich")]
        public void successful_search(string title, string expected)
        {
            var result = Subject.SearchForNewMovie(title).ToList();

            result.Should().NotBeEmpty();

            result[0].Title.Should().Be(expected);

            ExceptionVerification.IgnoreWarns();
        }

        [TestCase("tmdbid:")]
        [TestCase("tmdbid: 99999999999999999999")]
        [TestCase("tmdbid: 0")]
        [TestCase("tmdbid: -12")]
        [TestCase("adjalkwdjkalwdjklawjdlKAJD;EF")]
        public void no_search_result(string term)
        {
            var result = Subject.SearchForNewMovie(term);
            result.Should().BeEmpty();

            ExceptionVerification.IgnoreWarns();
        }
    }
}
