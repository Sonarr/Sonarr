using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Indexers.Newznab;
using NzbDrone.Core.Test.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Indexers;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.Test.IndexerTests.NewznabTests
{
    [TestFixture]
    public class OzNzbParserFixture : CoreTest<OzNzbRssParser>
    {
        private String _xml;

        [SetUp]
        public void SetUp()
        {
            _xml = ReadAllText(@"Files\RSS\oznzb.xml");
        }

        private IndexerResponse GetResponse()
        {
            return new IndexerResponse(null, new HttpResponse(null, new HttpHeader(), _xml));
        }

        [Test]
        public void should_return_upvotes_if_available()
        {
            var result = Subject.ParseResponse(GetResponse());
            
            result.First().UserRatings.UpVotes.Should().HaveValue();
            result.First().UserRatings.UpVotes.Should().Be(2);
        }

        [Test]
        public void should_not_return_upvotes_if_unavailable()
        {
            _xml = _xml.Replace("oz_up_votes", "__ignore__");

            var result = Subject.ParseResponse(GetResponse());

            result.First().UserRatings.UpVotes.Should().NotHaveValue();
        }

        [Test]
        public void should_not_return_ratings_if_unavailable()
        {
            _xml = _xml.Replace("\"oz_video_quality_rating\" value=\"6\"", "\"oz_video_quality_rating\" value=\"-\"");
            _xml = _xml.Replace("\"oz_audio_quality_rating\" value=\"6\"", "\"oz_audio_quality_rating\" value=\"-\"");

            var result = Subject.ParseResponse(GetResponse());

            result.First().UserRatings.VideoRating.Should().NotHaveValue();
            result.First().UserRatings.AudioRating.Should().NotHaveValue();
        }

        [TestCase("1", 0.0)]
        [TestCase("10", 1.0)]
        [TestCase("5", 4 / 9.0)]
        [TestCase("8.6667", 7.6667 / 9.0)]
        public void should_normalize_video_and_audio_rating(String rating, Double expectedFactor)
        {
            _xml = _xml.Replace("\"oz_video_quality_rating\" value=\"6\"", "\"oz_video_quality_rating\" value=\"" + rating + "\"");
            _xml = _xml.Replace("\"oz_audio_quality_rating\" value=\"6\"", "\"oz_audio_quality_rating\" value=\"" + rating + "\"");

            var result = Subject.ParseResponse(GetResponse());

            result.First().UserRatings.VideoRating.Should().HaveValue();
            result.First().UserRatings.AudioRating.Should().HaveValue();
            result.First().UserRatings.VideoRating.Should().BeApproximately(expectedFactor, 0.001);
            result.First().UserRatings.AudioRating.Should().BeApproximately(expectedFactor, 0.001);
        }
    }
}
