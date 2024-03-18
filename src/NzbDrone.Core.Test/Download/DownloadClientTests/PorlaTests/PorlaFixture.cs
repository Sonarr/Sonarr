using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients.LibTorrent.Models;
using NzbDrone.Core.Download.Clients.Porla;
using NzbDrone.Core.Download.Clients.Porla.Models;
using NzbDrone.Core.MediaFiles.TorrentInfo;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.PorlaTests
{
    [TestFixture]
    [Description("Tests the Porla Download Client")]
    public class PorlaFixture : DownloadClientFixtureBase<Porla>
    {
        private const string _somehash = "dd8255ecdc7ca55fb0bbf81323d87062db1f6d1c";
        protected PorlaTorrentDetail _queued;
        protected PorlaTorrentDetail _downloading;
        protected PorlaTorrentDetail _paused;

        // protected PorlaTorrentDetail _failed;
        // protected PorlaTorrentDetail _completed;
        protected PorlaTorrentDetail _seeding;

        private static readonly IList<string> SomeTags = new System.Collections.Generic.List<string> { "sometag", "someothertag" };
        private static readonly IList<string> DefaultTags = new System.Collections.Generic.List<string> { "apply_ip_filter", "auto_managed" };
        private static readonly IList<string> DefaultPausedTags = DefaultTags.Append("paused").ToList();

        [SetUp]
        public void Setup()
        {
            Subject.Definition = new DownloadClientDefinition();
            Subject.Definition.Settings = new PorlaSettings();

            _queued = new PorlaTorrentDetail
            {
                ActiveDuration = 1L,
                AllTimeDownload = 0L,
                AllTimeUpload = 0L,
                Category = "sonarr-tv",
                DownloadRate = 0,
                Error = null,
                ETA = -1L,
                FinishedDuration = 0L,
                Flags = new (DefaultTags),
                InfoHash = new ("HASH", null),
                LastDownload = -1L,
                LastUpload = -1L,
                ListPeers = 1,
                ListSeeds = 0,
                Metadata = null, // not exactly correct, should be json `{}`
                MovingStorage = false,
                Name = _title,
                NumPeers = 0,
                NumSeeds = 0,
                Progress = 0.0f,
                QueuePosition = 0,
                Ratio = 0.0d,
                SavePath = "/tmp",
                SeedingDuration = 0L,
                Session = "default",
                Size = 0L,
                State = LibTorrentStatus.downloading_metadata,
                Tags = new (SomeTags),  // do we have a remote episode aviable? can I use CreateRemoteEpisode
                Total = 0L,
                TotalDone = 0L,
                UploadRate = 0
            };

            _downloading = new PorlaTorrentDetail
            {
                ActiveDuration = 10L,
                AllTimeDownload = 100000000L,
                AllTimeUpload = 0L,
                Category = "sonarr-tv",
                DownloadRate = 2000000,
                Error = null,
                ETA = 200L,
                FinishedDuration = 0L,
                Flags = new (DefaultTags),
                InfoHash = new ("HASH", null),
                LastDownload = 1L,
                LastUpload = -1L,
                ListPeers = 90,
                ListSeeds = 6,
                Metadata = null, // not exactly correct, should be json `{}`
                MovingStorage = false,
                Name = _title,
                NumPeers = 10,
                NumSeeds = 8,
                Progress = 0.5f,
                QueuePosition = 0,
                Ratio = 0.0d,
                SavePath = "/tmp",
                SeedingDuration = 0L,
                Session = "default",
                Size = 100000000L,
                State = LibTorrentStatus.downloading,
                Tags = new (SomeTags),
                Total = 100000000L,
                TotalDone = 150000000L, // normally bigger than AllTimeDownload. compression? but doesn't encryption add overhead?
                UploadRate = 100000
            };

            _paused = new PorlaTorrentDetail
            {
                ActiveDuration = 10L,
                AllTimeDownload = 100000000L,
                AllTimeUpload = 0L,
                Category = "sonarr-tv",
                DownloadRate = 0,
                Error = null,
                ETA = -1L,
                FinishedDuration = 0L,
                Flags = new (DefaultPausedTags),  // "paused" should now exist in the flags.
                InfoHash = new ("HASH", null),
                LastDownload = 1L,
                LastUpload = -1L,
                ListPeers = 90,
                ListSeeds = 6,
                Metadata = null, // not exactly correct, should be json `{}`
                MovingStorage = false,
                Name = _title,
                NumPeers = 0,   // paused so this should be 0
                NumSeeds = 0,   // paused so this should be 0
                Progress = 0.5f,
                QueuePosition = 0,  // seems to still retain it's queue possition
                Ratio = 0.0d,
                SavePath = "/tmp",
                SeedingDuration = 0L,
                Session = "default",
                Size = 100000000L,
                State = LibTorrentStatus.downloading,  // LibTorrent does not set a state for paused
                Tags = new (SomeTags),
                Total = 100000000L,
                TotalDone = 150000000L,
                UploadRate = 0
            };

            // _failed = new PorlaTorrentDetail
            // {
            //     ActiveDuration = 10L,
            //     AllTimeDownload = 100000000L,
            //     AllTimeUpload = 0L,
            //     Category = "sonarr-tv",
            //     DownloadRate = 2000000,
            //     Error = null,  // pain: need an example
            //     ETA = 200L,
            //     FinishedDuration = 0L,
            //     Flags = new (DefaultTags),
            //     InfoHash = new ("HASH", null),
            //     LastDownload = 1L,
            //     LastUpload = -1L,
            //     ListPeers = 90,
            //     ListSeeds = 6,
            //     Metadata = new (null),
            //     MovingStorage = false,
            //     Name = _title,
            //     NumPeers = 10,
            //     NumSeeds = 8,
            //     Progress = 0.5f,
            //     QueuePosition = 0,
            //     Ratio = 0.0d,
            //     SavePath = "/tmp",
            //     SeedingDuration = 0L,
            //     Session = "default",
            //     Size = 100000000L,
            //     State = LibTorrentStatus.downloading,  // ?
            //     Tags = new (SomeTags),
            //     Total = 100000000L,
            //     TotalDone = 150000000L,
            //     UploadRate = 100000
            // };

            _seeding = new PorlaTorrentDetail
            {
                ActiveDuration = 120L,
                AllTimeDownload = 200000000L,
                AllTimeUpload = 100000000L,
                Category = "sonarr-tv",
                DownloadRate = 100,  // this seems to always be doing something / might be frozen at last value
                Error = null,
                ETA = -1L,  // we are done so eta is infinate
                FinishedDuration = 100L,
                Flags = new (DefaultTags),
                InfoHash = new ("HASH", null),
                LastDownload = 100L,
                LastUpload = 10L,
                ListPeers = 128,
                ListSeeds = 16,
                Metadata = null,
                MovingStorage = false,
                Name = _title,
                NumPeers = 1,
                NumSeeds = 2,
                Progress = 1.0f,
                QueuePosition = -1,
                Ratio = 1.0d,
                SavePath = "/tmp",
                SeedingDuration = 666L,
                Session = "default",
                Size = 190000000L,  // usually a little smaller than downloaded total
                State = LibTorrentStatus.seeding,  // double check this one
                Tags = new (SomeTags),  // do we have a remote episode aviable? can I use CreateRemoteEpisode
                Total = 0L,
                TotalDone = 190000000L, // after we are finished this should be the same as `size`
                UploadRate = 100
            };

            Mocker.GetMock<ITorrentFileInfoReader>()
                  .Setup(s => s.GetHashFromTorrentFile(It.IsAny<byte[]>()))
                  .Returns(_somehash);

            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.Get(It.IsAny<HttpRequest>()))
                  .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), Array.Empty<byte>()));
        }

        protected void GivenFailedDownload()
        {
            Mocker.GetMock<IPorlaProxy>()
                .Setup(s => s.AddMagnetTorrent(It.IsAny<PorlaSettings>(), It.IsAny<string>(), SomeTags))
                .Throws<InvalidOperationException>();

            Mocker.GetMock<IPorlaProxy>()
                .Setup(s => s.AddTorrentFile(It.IsAny<PorlaSettings>(), It.IsAny<byte[]>(), SomeTags))
                .Throws<InvalidOperationException>();
        }

        protected void GivenSuccessfulDownload()
        {
            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.GetAsync(It.IsAny<HttpRequest>()))
                  .Returns<HttpRequest>(r => Task.FromResult(new HttpResponse(r, new HttpHeader(), new byte[1000])));

            Mocker.GetMock<IPorlaProxy>()
                .Setup(s => s.AddMagnetTorrent(It.IsAny<PorlaSettings>(), It.IsAny<string>(), SomeTags))
                .Callback(PrepareClientToReturnQueuedItem);

            Mocker.GetMock<IPorlaProxy>()
                .Setup(s => s.AddTorrentFile(It.IsAny<PorlaSettings>(), It.IsAny<byte[]>(), SomeTags))
                .Returns(new PorlaTorrent(_somehash, null))
                .Callback(PrepareClientToReturnQueuedItem);
        }

        protected virtual void GivenTorrents(ReadOnlyCollection<PorlaTorrentDetail> torrents)
        {
            torrents ??= new ReadOnlyCollection<PorlaTorrentDetail>(new List<PorlaTorrentDetail>());

            Mocker.GetMock<IPorlaProxy>()
                .Setup(s => s.ListTorrents(It.IsAny<PorlaSettings>(), 0, int.MaxValue))
                .Returns(torrents);
        }

        protected void PrepareClientToReturnQueuedItem()
        {
            GivenTorrents(new ReadOnlyCollection<PorlaTorrentDetail>(
                new List<PorlaTorrentDetail> { _queued }));
        }

        protected void PrepareClientToReturnDownloadingItem()
        {
            GivenTorrents(new ReadOnlyCollection<PorlaTorrentDetail>(
                new List<PorlaTorrentDetail> { _downloading }));
        }

        // protected void PrepareClientToReturnFailedItem()
        // {
        //     GivenTorrents(new ReadOnlyCollection<PorlaTorrentDetail>(
        //         new List<PorlaTorrentDetail> { _failed }));
        // }

        // protected void PrepareClientToReturnCompletedItem()
        // {
        //      GivenTorrents(new ReadOnlyCollection<PorlaTorrentDetail>(
        //         new List<PorlaTorrentDetail> { _completed }));
        // }

        protected void PrepareClientToReturnSeedingItem()
        {
             GivenTorrents(new ReadOnlyCollection<PorlaTorrentDetail>(
                new List<PorlaTorrentDetail> { _seeding }));
        }

        protected void PrepareClientToReturnPausedItem()
        {
             GivenTorrents(new ReadOnlyCollection<PorlaTorrentDetail>(
                new List<PorlaTorrentDetail> { _paused }));
        }

        [Test]
        public void queued_item_should_have_required_properties()
        {
            PrepareClientToReturnQueuedItem();
            var item = Subject.GetItems().Single();
            VerifyQueued(item);
        }

        [Test]
        public void downloading_item_should_have_required_properties()
        {
            PrepareClientToReturnDownloadingItem();
            var item = Subject.GetItems().Single();
            VerifyDownloading(item);
        }

        // NOTE: We don't have an example yet
        // [Test]
        // public void failed_item_should_have_required_properties()
        // {
        //     PrepareClientToReturnFailedItem();
        //     var item = Subject.GetItems().Single();
        //     VerifyWarning(item);
        // }

        // NOTE: Looks like parent class requires Zero (0) for time left? Porla (LibTorrent) send -1 to indicate infinity (done)
        // We are considering a "completed" torrent as one in seeding progress
        [Test]
        public void completed_seeding_download_should_have_required_properties()
        {
            PrepareClientToReturnSeedingItem();
            var item = Subject.GetItems().Single();
            VerifyCompleted(item);

            item.CanBeRemoved.Should().BeTrue();
            item.CanMoveFiles.Should().BeTrue();
        }

        [Test]
        public void paused_download_should_have_required_properties()
        {
            PrepareClientToReturnPausedItem();
            var item = Subject.GetItems().Single();
            VerifyPaused(item);
        }

        [Test]
        public async Task download_should_return_unique_id()
        {
            GivenSuccessfulDownload();

            var remoteEpisode = CreateRemoteEpisode();

            var id = await Subject.Download(remoteEpisode, CreateIndexer());

            id.Should().NotBeNullOrEmpty();
        }

        // TODO: figure out presets
        [Test]
        public void should_return_status_with_outputdirs_with_no_set_preset()
        {
            // var emptyPresets = new PorlaPreset();
            var emptyPresetsDict = new Dictionary<string, PorlaPreset>();
            var emptyROPresetsDict = new ReadOnlyDictionary<string, PorlaPreset>(emptyPresetsDict);
            var someDir = "/tmp";

            // TODO: Idk how to mock the settings :( object. I want to know what the TvDirectory object is to use in the test below.
            Mocker.GetMock<IPorlaProxy>()
                .Setup(v => v.ListPresets(It.Is<PorlaSettings>(s => s.TvDirectory == someDir)))
                .Returns(emptyROPresetsDict);

            var result = Subject.GetStatus();

            result.IsLocalhost.Should().BeTrue();
            result.OutputRootFolders.Should().NotBeNull();
            result.OutputRootFolders.First().Should().Be(someDir);
        }

        [Test]
        public void should_return_status_with_outputdirs_with_default_preset()
        {
            var someDir = "/tmp/downloads";

            // TODO: Should this be a It.Is<>(); ?
            var presetWithSavePath = new PorlaPreset()
            {
                SavePath = someDir
            };
            var defaultPresetsDict = new Dictionary<string, PorlaPreset>
            {
                { "default", presetWithSavePath }
            };
            var defaultROPresetsDict = new ReadOnlyDictionary<string, PorlaPreset>(defaultPresetsDict);

            Mocker.GetMock<IPorlaProxy>()
                .Setup(v => v.ListPresets(It.Is<PorlaSettings>(s => s.TvDirectory == someDir && s.Preset == "default")))
                .Returns(defaultROPresetsDict);

            var result = Subject.GetStatus();

            result.IsLocalhost.Should().BeTrue();
            result.OutputRootFolders.Should().NotBeNull();
            result.OutputRootFolders.First().Should().Be(someDir);
        }

        [Test]
        public void should_return_status_with_outputdirs_with_alt_preset()
        {
            var someDir = "/home/user/downloads";
            var someOtherDir = "/data/downloads";
            var presetWithSomeSavePath = new PorlaPreset()
            {
                SavePath = someDir
            };
            var presetWithSomeOtherSavePath = new PorlaPreset()
            {
                SavePath = someOtherDir
            };
            var comboPresetsDict = new Dictionary<string, PorlaPreset>
            {
                { "default", presetWithSomeSavePath },
                { "alternative", presetWithSomeOtherSavePath }
            };
            var comboROPresetsDict = new ReadOnlyDictionary<string, PorlaPreset>(comboPresetsDict);

            Mocker.GetMock<IPorlaProxy>()
                .Setup(v => v.ListPresets(It.Is<PorlaSettings>(s => s.TvDirectory == someOtherDir && s.Preset == "alternative")))
                .Returns(comboROPresetsDict);

            var result = Subject.GetStatus();

            result.IsLocalhost.Should().BeTrue();
            result.OutputRootFolders.Should().NotBeNull();
            result.OutputRootFolders.First().Should().Be(someOtherDir);
        }

        [Test]
        public void GetItems_should_ignore_torrents_with_a_different_category()
        {
            // TODO: should probs deep copy _downloading
            var someDownloadingTorrent = new PorlaTorrentDetail
            {
                ActiveDuration = 10L,
                AllTimeDownload = 100000000L,
                AllTimeUpload = 0L,
                Category = "some-other-category",
                DownloadRate = 2000000,
                Error = null,
                ETA = 200L,
                FinishedDuration = 0L,
                Flags = new (DefaultTags),
                InfoHash = new ("HASH", null),
                LastDownload = 1L,
                LastUpload = -1L,
                ListPeers = 90,
                ListSeeds = 6,
                Metadata = null,
                MovingStorage = false,
                Name = _title,
                NumPeers = 10,
                NumSeeds = 8,
                Progress = 0.5f,
                QueuePosition = -0,
                Ratio = 0.0d,
                SavePath = "/tmp",
                SeedingDuration = 0L,
                Session = "default",
                Size = 100000000L,
                State = LibTorrentStatus.downloading,
                Tags = new (SomeTags),
                Total = 100000000L,
                TotalDone = 150000000L,
                UploadRate = 100000
            };

            var torrents = new PorlaTorrentDetail[] { someDownloadingTorrent };
            var roTorrents = new ReadOnlyCollection<PorlaTorrentDetail>(torrents);
            Mocker.GetMock<IPorlaProxy>()
                .Setup(v => v.ListTorrents(It.IsAny<PorlaSettings>(), 0, int.MaxValue))
                .Returns(roTorrents);

            Subject.GetItems().Should().BeEmpty();
        }

        // We are not incompatible yet, when we are, you can uncomment this
        // [Test]
        // public void Test_should_return_validation_failure_for_old_Porla()
        // {
        //     var systemInfo = new PorlaSysVersions()
        //     {
        //         Porla = new PorlaSysVersionsPorla()
        //         {
        //             Version = "0.37.0"
        //         }
        //     };
        //
        //     Mocker.GetMock<IPorlaProxy>()
        //        .Setup(v => v.GetSysVersion(It.IsAny<PorlaSettings>()))
        //        .Returns(systemInfo);
        //
        //     var result = Subject.Test();
        //
        //     result.Errors.Count.Should().Be(1);
        // }
    }
}
