using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Integration.Test.Client;
using Sonarr.Api.V3.Episodes;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class CalendarFixture : IntegrationTest
    {
        public ClientBase<EpisodeResource> Calendar;

        protected override void InitRestClients()
        {
            base.InitRestClients();

            Calendar = new ClientBase<EpisodeResource>(RestClient, ApiKey, "calendar");
        }

        [Test]
        public void should_be_able_to_get_episodes()
        {
            var series = EnsureSeries(266189, "The Blacklist", true);

            var request = Calendar.BuildRequest();
            request.AddParameter("start", new DateTime(2015, 10, 1).ToString("s") + "Z");
            request.AddParameter("end", new DateTime(2015, 10, 3).ToString("s") + "Z");
            var items = Calendar.Get<List<EpisodeResource>>(request);

            items = items.Where(v => v.SeriesId == series.Id).ToList();

            items.Should().HaveCount(1);
            items.First().Title.Should().Be("The Troll Farmer");
        }

        [Test]
        public void should_not_be_able_to_get_unmonitored_episodes()
        {
            var series = EnsureSeries(266189, "The Blacklist", false);

            var request = Calendar.BuildRequest();
            request.AddParameter("start", new DateTime(2015, 10, 1).ToString("s") + "Z");
            request.AddParameter("end", new DateTime(2015, 10, 3).ToString("s") + "Z");
            request.AddParameter("unmonitored", "false");
            var items = Calendar.Get<List<EpisodeResource>>(request);

            items = items.Where(v => v.SeriesId == series.Id).ToList();

            items.Should().BeEmpty();
        }

        [Test]
        public void should_be_able_to_get_unmonitored_episodes()
        {
            var series = EnsureSeries(266189, "The Blacklist", false);

            var request = Calendar.BuildRequest();
            request.AddParameter("start", new DateTime(2015, 10, 1).ToString("s") + "Z");
            request.AddParameter("end", new DateTime(2015, 10, 3).ToString("s") + "Z");
            request.AddParameter("unmonitored", "true");
            var items = Calendar.Get<List<EpisodeResource>>(request);

            items = items.Where(v => v.SeriesId == series.Id).ToList();

            items.Should().HaveCount(1);
            items.First().Title.Should().Be("The Troll Farmer");
        }
    }
}
