using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.IPTorrents
{
    public class IPTorrentsRssParser : BasicTorrentRssParser
    {
        protected override XElement GetTorrentElement(XElement item)
        {
            return item;
        }

        protected override Int64 GetSize(XElement torrentElement)
        {
            var description = torrentElement.Element("description").Value;
            var sizeString = description.Substring(description.LastIndexOf("Size:") + 5).Trim();

            Double sizeValue = Double.Parse(sizeString.Split(' ').First(), CultureInfo.InvariantCulture);
            Int64 fileSize = 0;

            if (sizeString.IndexOf("GB") != -1)
            {
                fileSize = Convert.ToInt64(sizeValue * 1073741824);
            }
            else if (sizeString.IndexOf("MB") != -1)
            {
                fileSize = Convert.ToInt64(sizeValue * 1048576);
            }
            else if (sizeString.IndexOf("KB") != -1) // KB ??? need to find examples
            {
                fileSize = Convert.ToInt64(sizeValue * 1024);
            }
            else // bytes? ??? need to find examples
            {
                fileSize = Convert.ToInt64(sizeValue);
            }

            return fileSize;
        }
    }
}