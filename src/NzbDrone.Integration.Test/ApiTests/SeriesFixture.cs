using FluentAssertions;
using NUnit.Framework;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class SeriesFixture : IntegrationTest
    {
        [Test, Order(0)]
        public void add_series_with_tags_should_store_them()
        {
            EnsureNoSeries(266189, "The Blacklist");
            var tag = EnsureTag("abc");

            var series = Series.Lookup("tvdb:266189").Single();

            series.ProfileId = 1;
            series.LanguageProfileId = 1;
            series.Path = Path.Combine(SeriesRootFolder, series.Title);
            series.Tags = new HashSet<int>();
            series.Tags.Add(tag.Id);

            var result = Series.Post(series);

            result.Should().NotBeNull();
            result.Tags.Should().Equal(tag.Id);
        }

        [Test, Order(0)]
        public void add_series_without_profileid_should_return_badrequest()
        {
            EnsureNoSeries(266189, "The Blacklist");

            var series = Series.Lookup("tvdb:266189").Single();

            series.Path = Path.Combine(SeriesRootFolder, series.Title);

            Series.InvalidPost(series);
        }

        [Test, Order(0)]
        public void add_series_without_path_should_return_badrequest()
        {
            EnsureNoSeries(266189, "The Blacklist");

            var series = Series.Lookup("tvdb:266189").Single();

            series.ProfileId = 1;

            Series.InvalidPost(series);
        }

        [Test, Order(1)]
        public void add_series()
        {
            EnsureNoSeries(266189, "The Blacklist");

            var series = Series.Lookup("tvdb:266189").Single();

            series.ProfileId = 1;
            series.LanguageProfileId = 1;
            series.Path = Path.Combine(SeriesRootFolder, series.Title);

            var result = Series.Post(series);

            result.Should().NotBeNull();
            result.Id.Should().NotBe(0);
            result.ProfileId.Should().Be(1);
            result.LanguageProfileId.Should().Be(1);
            result.Path.Should().Be(Path.Combine(SeriesRootFolder, series.Title));
        }


        [Test, Order(2)]
        public void get_all_series()
        {
            EnsureSeries(266189, "The Blacklist");
            EnsureSeries(73065, "Archer");

            Series.All().Should().NotBeNullOrEmpty();
            Series.All().Should().Contain(v => v.TvdbId == 73065);
            Series.All().Should().Contain(v => v.TvdbId == 266189);
        }

        [Test, Order(2)]
        public void get_series_by_id()
        {
            var series = EnsureSeries(266189, "The Blacklist");

            var result = Series.Get(series.Id);

            result.TvdbId.Should().Be(266189);
        }

        [Test]
        public void get_series_by_unknown_id_should_return_404()
        {
            var result = Series.InvalidGet(1000000);
        }

        [Test, Order(2)]
        public void update_series_profile_id()
        {
            var series = EnsureSeries(266189, "The Blacklist");

            var profileId = 1;
            if (series.ProfileId == profileId)
            {
                profileId = 2;
            }

            series.ProfileId = profileId;

            var result = Series.Put(series);

            Series.Get(series.Id).ProfileId.Should().Be(profileId);
        }

        [Test, Order(3)]
        public void update_series_monitored()
        {
            var series = EnsureSeries(266189, "The Blacklist", false);

            series.Monitored.Should().BeFalse();
            series.Seasons.First().Monitored.Should().BeFalse();

            series.Monitored = true;
            series.Seasons.ForEach(season =>
            {
                season.Monitored = true;
            });

            var result = Series.Put(series);

            result.Monitored.Should().BeTrue();
            result.Seasons.First().Monitored.Should().BeTrue();
        }

        [Test, Order(3)]
        public void update_series_tags()
        {
            var series = EnsureSeries(266189, "The Blacklist");
            var tag = EnsureTag("abc");

            if (series.Tags.Contains(tag.Id))
            {
                series.Tags.Remove(tag.Id);

                var result = Series.Put(series);
                Series.Get(series.Id).Tags.Should().NotContain(tag.Id);
            }
            else
            {
                series.Tags.Add(tag.Id);

                var result = Series.Put(series);
                Series.Get(series.Id).Tags.Should().Contain(tag.Id);
            }
        }

        [Test, Order(4)]
        public void delete_series()
        {
            var series = EnsureSeries(266189, "The Blacklist");

            Series.Get(series.Id).Should().NotBeNull();

            Series.Delete(series.Id);

            Series.All().Should().NotContain(v => v.TvdbId == 266189);
        }
    }
}