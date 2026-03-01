using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Instrumentation;

namespace NzbDrone.Common.Test.InstrumentationTests
{
    [TestFixture]
    public class CleanseLogMessageFixture
    {
        // Indexer Urls
        [TestCase(@"https://iptorrents.com/torrents/rss?u=mySecret;tp=mySecret;l5;download")]
        [TestCase(@"http://rss.torrentleech.org/mySecret")]
        [TestCase(@"https://rss24h.torrentleech.org/mySecret")]
        [TestCase(@"http://rss.torrentleech.org/rss/download/12345/01233210/file.name-RLSGRP.torrent")]
        [TestCase(@"https://www.torrentleech.org/rss/download/12345/01233210/file.name-RLSGRP.torrent")]
        [TestCase(@"https://rss.omgwtfnzbs.org/rss-search.php?catid=19,20&user=sonarr&api=mySecret&eng=1")]
        [TestCase(@"https://dognzb.cr/fetch/2b51db35e1912ffc138825a12b9933d2/2b51db35e1910123321025a12b9933d2")]
        [TestCase(@"https://baconbits.org/feeds.php?feed=torrents_tv&user=12345&auth=2b51db35e1910123321025a12b9933d2&passkey=mySecret&authkey=2b51db35e1910123321025a12b9933d2")]
        [TestCase(@"http://127.0.0.1:9117/dl/indexername?jackett_apikey=flwjiefewklfjacketmySecretsdfldskjfsdlk&path=we0re9f0sdfbase64sfdkfjsdlfjk&file=The+Torrent+File+Name.torrent")]
        [TestCase(@"http://nzb.su/getnzb/2b51db35e1912ffc138825a12b9933d2.nzb&i=37292&r=2b51db35e1910123321025a12b9933d2")]
        [TestCase(@"http://nzb.su/rss?t=-2&dl=1&i=37292&r=2b51db35e1910123321025a12b9933d2")]
        [TestCase(@"https://b-hd.me/torrent/download/auto.343756.is1t1pl127p1sfwur8h4kgyhg1wcsn05")]
        [TestCase(@"https://b-hd.me/torrent/download/a-slug-in-the-url.343756.is1t1pl127p1sfwur8h4kgyhg1wcsn05")]

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
        [TestCase(@",{""download_location"": ""/Users/mySecret/Downloads""}")]
        [TestCase(@"auth.login(""mySecret"")")]

        // Download Station
        [TestCase(@"webapi/entry.cgi?api=(removed)&version=2&method=login&account=01233210&passwd=mySecret&format=sid&session=DownloadStation")]

        // BroadcastheNet
        [TestCase(@"method: ""getTorrents"", ""params"": [ ""mySecret"",")]
        [TestCase(@"getTorrents(""mySecret"", [asdfasdf], 100, 0)")]
        [TestCase(@"""DownloadURL"":""https:\/\/broadcasthe.net\/torrents.php?action=download&id=123&authkey=mySecret&torrent_pass=mySecret""")]

        // Plex
        [TestCase(@" http://localhost:32400/library/metadata/12345/refresh?X-Plex-Client-Identifier=1234530f-422f-4aac-b6b3-01233210aaaa&X-Plex-Product=Sonarr&X-Plex-Platform=Windows&X-Plex-Platform-Version=7&X-Plex-Device-Name=Sonarr&X-Plex-Version=3.0.3.833&X-Plex-Token=mySecret")]

        // Internal
        [TestCase(@"OutputPath=/home/mySecret/Downloads")]
        [TestCase(@"OutputPath=/Users/mySecret/Downloads")]
        [TestCase("Hardlinking episode file: /home/mySecret/Downloads to /media/abc.mkv")]
        [TestCase("Hardlinking episode file: /Users/mySecret/Downloads to /media/abc.mkv")]
        [TestCase("Hardlink '/home/mySecret/Downloads/abs.mkv' to '/media/abc.mkv' failed.")]
        [TestCase("Hardlink '/Users/mySecret/Downloads/abs.mkv' to '/media/abc.mkv' failed.")]
        [TestCase("/sonarr/signalr/messages/negotiate?access_token=1234530f422f4aacb6b301233210aaaa&negotiateVersion=1")]
        [TestCase(@"[Info] MigrationController: *** Migrating Database=sonarr-main;Host=postgres14;Username=mySecret;Password=mySecret;Port=5432;Enlist=False ***")]
        [TestCase(@"[Info] MigrationController: *** Migrating Database=sonarr-main;Host=postgres14;Username=mySecret;Password=mySecret;Port=5432;token=mySecret;Enlist=False&username=mySecret;mypassword=mySecret;mypass=shouldkeep1;test_token=mySecret;password=123%@%_@!#^#@;use_password=mySecret;get_token=shouldkeep2;usetoken=shouldkeep3;passwrd=mySecret;")]

        // Announce URLs (passkeys) Magnet & Tracker
        [TestCase(@"magnet_uri"":""magnet:?xt=urn:btih:9pr04sgkillroyimaveql2tyu8xyui&dn=&tr=https%3a%2f%2fxxx.yyy%2f9pr04sg601233210IMAveQL2tyu8xyui%2fannounce""}")]
        [TestCase(@"magnet_uri"":""magnet:?xt=urn:btih:9pr04sgkillroyimaveql2tyu8xyui&dn=&tr=https%3a%2f%2fxxx.yyy%2ftracker.php%2f9pr04sg601233210IMAveQL2tyu8xyui%2fannounce""}")]
        [TestCase(@"magnet_uri"":""magnet:?xt=urn:btih:9pr04sgkillroyimaveql2tyu8xyui&dn=&tr=https%3a%2f%2fxxx.yyy%2fannounce%2f9pr04sg601233210IMAveQL2tyu8xyui""}")]
        [TestCase(@"magnet_uri"":""magnet:?xt=urn:btih:9pr04sgkillroyimaveql2tyu8xyui&dn=&tr=https%3a%2f%2fxxx.yyy%2fannounce.php%3fpasskey%3d9pr04sg601233210IMAveQL2tyu8xyui""}")]
        [TestCase(@"tracker"":""https://xxx.yyy/9pr04sg601233210IMAveQL2tyu8xyui/announce""}")]
        [TestCase(@"tracker"":""https://xxx.yyy/tracker.php/9pr04sg601233210IMAveQL2tyu8xyui/announce""}")]
        [TestCase(@"tracker"":""https://xxx.yyy/announce/9pr04sg601233210IMAveQL2tyu8xyui""}")]
        [TestCase(@"tracker"":""https://xxx.yyy/announce.php?passkey=9pr04sg601233210IMAveQL2tyu8xyui""}")]
        [TestCase(@"tracker"":""http://xxx.yyy/announce.php?passkey=9pr04sg601233210IMAveQL2tyu8xyui"",""info"":""http://xxx.yyy/info?a=b""")]

        // Webhooks - Notifiarr
        [TestCase(@"https://xxx.yyy/api/v1/notification/sonarr/9pr04sg6-0123-3210-imav-eql2tyu8xyui")]

        // Discord
        [TestCase(@"https://discord.com/api/webhooks/mySecret")]
        [TestCase(@"https://discord.com/api/webhooks/mySecret/01233210")]

        // Telegram
        [TestCase(@"https://api.telegram.org/bot1234567890:mySecret/sendmessage: chat_id=123456&parse_mode=HTML&text=<text>")]
        [TestCase(@"https://api.telegram.org/bot1234567890:mySecret/")]

        public void should_clean_message(string message)
        {
            var cleansedMessage = CleanseLogMessage.Cleanse(message);

            cleansedMessage.Should().NotContain("mySecret");
            cleansedMessage.Should().NotContain("123%@%_@!#^#@");
            cleansedMessage.Should().NotContain("01233210");
        }

        [TestCase(@"[Info] MigrationController: *** Migrating Database=sonarr-main;Host=postgres14;Username=mySecret;Password=mySecret;Port=5432;token=mySecret;Enlist=False&username=mySecret;mypassword=mySecret;mypass=shouldkeep1;test_token=mySecret;password=123%@%_@!#^#@;use_password=mySecret;get_token=shouldkeep2;usetoken=shouldkeep3;passwrd=mySecret;")]
        public void should_keep_message(string message)
        {
            var cleansedMessage = CleanseLogMessage.Cleanse(message);

            cleansedMessage.Should().NotContain("mySecret");
            cleansedMessage.Should().NotContain("123%@%_@!#^#@");
            cleansedMessage.Should().NotContain("01233210");

            cleansedMessage.Should().Contain("shouldkeep1");
            cleansedMessage.Should().Contain("shouldkeep2");
            cleansedMessage.Should().Contain("shouldkeep3");
        }

        [TestCase(@"Some message (from 32.2.3.5 user agent)")]
        [TestCase(@"Auth-Invalidated ip 32.2.3.5")]
        [TestCase(@"Auth-Success ip 32.2.3.5")]
        [TestCase(@"Auth-Logout ip 32.2.3.5")]
        public void should_clean_ipaddress(string message)
        {
            var cleansedMessage = CleanseLogMessage.Cleanse(message);

            cleansedMessage.Should().NotContain(".2.3.");
        }

        [TestCase(@"Some message (from 10.2.3.2 user agent)")]
        [TestCase(@"Auth-Unauthorized ip 32.2.3.5")]
        [TestCase(@"Auth-Failure ip 32.2.3.5")]
        public void should_not_clean_ipaddress(string message)
        {
            var cleansedMessage = CleanseLogMessage.Cleanse(message);

            cleansedMessage.Should().Be(message);
        }
    }
}
