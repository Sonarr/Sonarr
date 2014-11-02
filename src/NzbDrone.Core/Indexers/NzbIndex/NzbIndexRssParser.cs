using System;
using System.Linq;
using System.Xml.Linq;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.NzbIndex
{
    public class NzbIndexRssParser : RssParser
    {
        protected override ReleaseInfo PostProcess(XElement item, ReleaseInfo releaseInfo)
        {
            //Drop out the password protected items
            var description = item.TryGetValue("description", String.Empty);
            return (!string.IsNullOrWhiteSpace(description) && !description.Contains("Password protected"))
                ? releaseInfo
                : null;
        }

        protected override string GetTitle(XElement item)
        {
            return GetFileFromUrl(GetDownloadUrl(item));
        }

        public virtual string GetFileFromUrl(string url)
        {
            return new Uri(url).Segments.Last();
        }

        protected override string GetInfoUrl(XElement item)
        {
            return item.Element("link").Value;
        }

        protected override string GetCommentUrl(XElement item)
        {
            var stdUrl = base.GetInfoUrl(item);
            //Grep nfo link if standard link is not available
            return (string.IsNullOrWhiteSpace(stdUrl)) ? GetNfoUrl(item) : stdUrl;
        }

        public virtual string GetNfoUrl(XElement item)
        {
            if (item == null) throw new ArgumentNullException("item");

            var description = item.TryGetValue("description", String.Empty);
            return !string.IsNullOrWhiteSpace(description) ? GetHrefLinks(description).FirstOrDefault() : null;
        }

    }
}
