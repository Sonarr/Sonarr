//http://stackoverflow.com/questions/210375/problems-reading-rss-with-c-and-net-3-5
//https://connect.microsoft.com/VisualStudio/feedback/details/325421/syndicationfeed-load-fails-to-parse-datetime-against-a-real-world-feeds-ie7-can-read

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Syndication;
using System.Xml;

namespace NzbDrone.Core.Providers.Indexer
{

    public class SyndicationFeedXmlReader : XmlTextReader
    {
        readonly string[] Rss20DateTimeHints = { "pubDate" };
        readonly string[] Atom10DateTimeHints = { "updated", "published", "lastBuildDate" };
        private bool isRss2DateTime = false;
        private bool isAtomDateTime = false;

        public SyndicationFeedXmlReader(Stream stream) : base(stream) { }

        public override bool IsStartElement(string localname, string ns)
        {
            isRss2DateTime = false;
            isAtomDateTime = false;

            if (Rss20DateTimeHints.Contains(localname)) isRss2DateTime = true;
            if (Atom10DateTimeHints.Contains(localname)) isAtomDateTime = true;

            return base.IsStartElement(localname, ns);
        }

        public override string ReadString()
        {
            string dateVal = base.ReadString();

            try
            {
                if (isRss2DateTime)
                {
                    MethodInfo objMethod = typeof(Rss20FeedFormatter).GetMethod("DateFromString", BindingFlags.NonPublic | BindingFlags.Static);
                    Debug.Assert(objMethod != null);
                    objMethod.Invoke(null, new object[] { dateVal, this });

                }
                if (isAtomDateTime)
                {
                    MethodInfo objMethod = typeof(Atom10FeedFormatter).GetMethod("DateFromString", BindingFlags.NonPublic | BindingFlags.Instance);
                    Debug.Assert(objMethod != null);
                    objMethod.Invoke(new Atom10FeedFormatter(), new object[] { dateVal, this });
                }
            }
            catch (TargetInvocationException)
            {
                DateTimeFormatInfo dtfi = CultureInfo.CurrentCulture.DateTimeFormat;
                return DateTimeOffset.UtcNow.ToString(dtfi.RFC1123Pattern);
            }

            return dateVal;

        }

    }
}
