using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients.JDownloader;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.Primewire
{
    public class PrimewirePageParser : IParseIndexerResponse
    {
        public PrimewireSettings Settings { get; set; }

        public PrimewirePageParser()
        {
        }

        public IList<ReleaseInfo> ParseResponse(IndexerResponse indexerResponse)
        {
            var content = indexerResponse.HttpResponse.Content;
            var releases = FindReleases(indexerResponse.HttpRequest.Url.ToString(), content);
            return releases;
        }

        readonly Regex linksPattern = new Regex("url=(?<url>.{1,100}?)&domain=(?<domain>.{1,100}?)&", RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.IgnoreCase);
        readonly Regex titleRegex = new Regex("<title>Watch \"(?<title>.*?)\".*?Season (?<season>\\d{1,3}) and Episode (?<episode>\\d{1,3})</title>", RegexOptions.Compiled);
        readonly Regex idRegex = new Regex("<a href=\"/tv-(?<id>\\d{1,10})-", RegexOptions.Compiled);
        private IList<ReleaseInfo> FindReleases(string url, string content)
        {
            var matches = linksPattern.Matches(content).Cast<Match>();
            var links = matches.Select(a => new
            {
                Url = Encoding.UTF8.GetString(Convert.FromBase64String(a.Groups["url"].Value)),
                Domain = Encoding.UTF8.GetString(Convert.FromBase64String(a.Groups["domain"].Value))
            }).Where(a => !a.Url.StartsWith("/")).ToList();

            if(!links.Any())
                return new List<ReleaseInfo>();

            DateTime airdate = DateTime.MinValue;
            var title = "";
            var id = "";

            var titleMatch = titleRegex.Match(content);
            if (titleMatch.Success) 
                title = string.Format("{0} S{1:00}E{2:00}", 
                    titleMatch.Groups["title"].Value, 
                    Convert.ToInt32(titleMatch.Groups["season"].Value), 
                    Convert.ToInt32(titleMatch.Groups["episode"].Value));

            var idMatch = idRegex.Match(content);
            if (idMatch.Success) id = idMatch.Groups["id"].Value;

            Regex airdateRegex = new Regex("Air Date:</strong></td>.*?<td>(?<date>.*?)</td>", RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.IgnoreCase);
            var airdateMatch = airdateRegex.Match(content);
            if (airdateMatch.Success)
            {
                IFormatProvider culture = new System.Globalization.CultureInfo("en-US", true);
                DateTime.TryParse(airdateMatch.Groups["date"].Value, culture, System.Globalization.DateTimeStyles.AdjustToUniversal, out airdate);
            }

            var info = url;
            var comments = info + "#comments";

            return links.Select((a,index) => new FilehosterInfo()
            {
                Guid = Guid.NewGuid().ToString(),
                InfoUrl = info,
                Title = title,
                DownloadUrl = a.Url.Replace("?v=",""),
                CommentUrl = comments,
                PublishDate = airdate
                
            }).AsEnumerable<ReleaseInfo>().ToList();
        }
    }
}