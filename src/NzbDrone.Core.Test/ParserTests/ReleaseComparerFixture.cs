using System;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Blocklisting;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class ReleaseComparerFixture
    {
        private const string Title = "Series.Title.S01E05.1080p.x265-ELiTE";
        private const long Size = 1352581934;

        private static readonly DateTime PublishedDate = new DateTime(2026, 2, 6, 15, 39, 0, DateTimeKind.Utc);

        private static ReleaseComparerModel Blocklisted(string indexer, DateTime? publishedDate)
        {
            return new ReleaseComparerModel(new Blocklist
            {
                SourceTitle = Title,
                Indexer = indexer,
                PublishedDate = publishedDate,
                Size = Size
            });
        }

        private static ReleaseInfo Release(string indexer, DateTime publishDate)
        {
            return new ReleaseInfo
            {
                Title = Title,
                Indexer = indexer,
                PublishDate = publishDate,
                Size = Size
            };
        }

        [Test]
        public void should_not_match_release_from_another_indexer_with_near_published_date()
        {
            var item = Blocklisted("NZBgeek", PublishedDate);
            var release = Release("NZBFinder", PublishedDate.AddSeconds(-10));

            ReleaseComparer.SameNzb(item, release).Should().BeFalse();
        }

        [Test]
        public void should_not_match_release_from_another_indexer_with_identical_published_date()
        {
            var item = Blocklisted("NZBgeek", PublishedDate);
            var release = Release("NZBFinder", PublishedDate);

            ReleaseComparer.SameNzb(item, release).Should().BeFalse();
        }

        [Test]
        public void should_match_same_indexer_with_identical_published_date()
        {
            var item = Blocklisted("NZBgeek", PublishedDate);
            var release = Release("NZBgeek", PublishedDate);

            ReleaseComparer.SameNzb(item, release).Should().BeTrue();
        }

        [Test]
        public void should_not_match_same_indexer_when_published_date_differs()
        {
            var item = Blocklisted("NZBgeek", PublishedDate);
            var release = Release("NZBgeek", PublishedDate.AddSeconds(-10));

            ReleaseComparer.SameNzb(item, release).Should().BeFalse();
        }

        [Test]
        public void should_not_match_same_indexer_when_item_has_no_published_date()
        {
            var item = Blocklisted("NZBgeek", null);
            var release = Release("NZBgeek", PublishedDate);

            ReleaseComparer.SameNzb(item, release).Should().BeFalse();
        }

        [Test]
        public void should_match_item_without_indexer_with_identical_published_date()
        {
            var item = Blocklisted(null, PublishedDate);
            var release = Release("NZBFinder", PublishedDate);

            ReleaseComparer.SameNzb(item, release).Should().BeTrue();
        }

        [Test]
        public void should_not_match_item_without_indexer_when_published_date_differs()
        {
            var item = Blocklisted(null, PublishedDate);
            var release = Release("NZBFinder", PublishedDate.AddSeconds(-10));

            ReleaseComparer.SameNzb(item, release).Should().BeFalse();
        }
    }
}
