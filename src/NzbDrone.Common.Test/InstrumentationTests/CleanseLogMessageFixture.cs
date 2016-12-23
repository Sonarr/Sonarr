using NUnit.Framework;
using NzbDrone.Common.Instrumentation;
using FluentAssertions;

namespace NzbDrone.Common.Test.InstrumentationTests
{
    [TestFixture]
    public class CleanseLogMessageFixture
    {
        // Indexer Urls
        [TestCase(@"https://iptorrents.com/torrents/rss?u=mySecret;tp=mySecret;l5;download")]
        [TestCase(@"http://rss.torrentleech.org/mySecret")]
        [TestCase(@"http://rss.torrentleech.org/rss/download/12345/01233210/filename.torrent")]
        [TestCase(@"http://www.bitmetv.org/rss.php?uid=mySecret&passkey=mySecret")]
        [TestCase(@"https://rss.omgwtfnzbs.org/rss-search.php?catid=19,20&user=sonarr&api=mySecret&eng=1")]
        [TestCase(@"https://dognzb.cr/fetch/2b51db35e1912ffc138825a12b9933d2/2b51db35e1910123321025a12b9933d2")]
        [TestCase(@"https://baconbits.org/feeds.php?feed=torrents_tv&user=12345&auth=2b51db35e1910123321025a12b9933d2&passkey=mySecret&authkey=2b51db35e1910123321025a12b9933d2")]
        // NzbGet
        [TestCase(@"{ ""Name"" : ""ControlUsername"", ""Value"" : ""mySecret"" }, { ""Name"" : ""ControlPassword"", ""Value"" : ""mySecret"" }, ")]
        [TestCase(@"{ ""Name"" : ""Server1.Username"", ""Value"" : ""mySecret"" }, { ""Name"" : ""Server1.Password"", ""Value"" : ""mySecret"" }, ")]
        // Sabnzbd
        [TestCase(@"http://127.0.0.1:1234/api/call?vv=1&apikey=mySecret")]
        [TestCase(@"http://127.0.0.1:1234/api/call?vv=1&ma_username=mySecret&ma_password=mySecret")]
        [TestCase(@"""config"":{""newzbin"":{""username"":""mySecret"",""password"":""mySecret""}")]
        [TestCase(@"""nzbxxx"":{""username"":""mySecret"",""apikey"":""mySecret""}")]
        [TestCase(@"""growl"":{""growl_password"":""mySecret"",""growl_server"":""""}")]
        [TestCase(@"""nzbmatrix"":{""username"":""mySecret"",""apikey"":""mySecret""}")]
        [TestCase(@"""misc"":{""username"":""mySecret"",""api_key"":""mySecret"",""password"":""mySecret"",""nzb_key"":""mySecret""}")]
        [TestCase(@"""servers"":[{""username"":""mySecret"",""password"":""mySecret""}]")]
        [TestCase(@"""misc"":{""email_account"":""mySecret"",""email_to"":[],""email_from"":"""",""email_pwd"":""mySecret""}")]
        // uTorrent
        [TestCase(@"http://localhost:9091/gui/?token=wThmph5l0ZXfH-a6WOA4lqiLvyjCP0FpMrMeXmySecret_VXBO11HoKL751MAAAAA&list=1")]
        [TestCase(@",[""boss_key"",0,""mySecret"",{""access"":""Y""}],[""boss_key_salt"",0,""mySecret"",{""access"":""W""}]")]
        [TestCase(@",[""webui.username"",2,""mySecret"",{""access"":""Y""}],[""webui.password"",2,""mySecret"",{""access"":""Y""}]")]
        [TestCase(@",[""webui.uconnect_username"",2,""mySecret"",{""access"":""Y""}],[""webui.uconnect_password"",2,""mySecret"",{""access"":""Y""}]")]
        [TestCase(@",[""proxy.proxy"",2,""mySecret"",{""access"":""Y""}]")]
        [TestCase(@",[""proxy.username"",2,""mySecret"",{""access"":""Y""}],[""proxy.password"",2,""mySecret"",{""access"":""Y""}]")]
        // Deluge
        [TestCase(@",{""download_location"": ""C:\Users\\mySecret mySecret\\Downloads""}")]
        [TestCase(@",{""download_location"": ""/home/mySecret/Downloads""}")]
        [TestCase(@"auth.login(""mySecret"")")]
        // BroadcastheNet
        [TestCase(@"method: ""getTorrents"", ""params"": [ ""mySecret"",")]
        [TestCase(@"getTorrents(""mySecret"", [asdfasdf], 100, 0)")]
        [TestCase(@"""DownloadURL"":""https:\/\/broadcasthe.net\/torrents.php?action=download&id=123&authkey=mySecret&torrent_pass=mySecret""")]
        public void should_clean_message(string message)
        {
            var cleansedMessage = CleanseLogMessage.Cleanse(message);

            cleansedMessage.Should().NotContain("mySecret");
            cleansedMessage.Should().NotContain("01233210");
        }
    }
}
