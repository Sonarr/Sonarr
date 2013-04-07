//http://stackoverflow.com/questions/210375/problems-reading-rss-with-c-and-net-3-5
//https://connect.microsoft.com/VisualStudio/feedback/details/325421/syndicationfeed-load-fails-to-parse-datetime-against-a-real-world-feeds-ie7-can-read

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Xml;
using NLog;

namespace NzbDrone.Core.Indexers
{
    public class SyndicationFeedXmlReader : XmlTextReader
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static readonly string[] rss20DateTimeHints = { "pubDate" };
        private static readonly string[] atom10DateTimeHints = { "updated", "published", "lastBuildDate" };
        private bool _isRss2DateTime;
        private bool _isAtomDateTime;

        private static readonly MethodInfo rss20FeedFormatterMethodInfo = typeof(Rss20FeedFormatter).GetMethod("DateFromString", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo atom10FeedFormatterMethodInfo = typeof(Atom10FeedFormatter).GetMethod("DateFromString", BindingFlags.NonPublic | BindingFlags.Instance);

        public SyndicationFeedXmlReader(Stream stream) : base(stream) { }

        public override bool IsStartElement(string localname, string ns)
        {
            _isRss2DateTime = rss20DateTimeHints.Contains(localname);
            _isAtomDateTime = atom10DateTimeHints.Contains(localname);

            CheckForError();       

            return base.IsStartElement(localname, ns);
        }

        public override string ReadString()
        {
            var dateVal = base.ReadString();

            try
            {
                if (_isRss2DateTime)
                {
                    rss20FeedFormatterMethodInfo.Invoke(null, new object[] { dateVal, this });
                }
                if (_isAtomDateTime)
                {
                    atom10FeedFormatterMethodInfo.Invoke(new Atom10FeedFormatter(), new object[] { dateVal, this });
                }
            }
            catch (TargetInvocationException e)
            {
                DateTime parsedDate;

                if (!DateTime.TryParse(dateVal, new CultureInfo("en-US"), DateTimeStyles.None, out parsedDate))
                {
                    parsedDate = DateTime.UtcNow;
                    logger.WarnException("Unable to parse Feed date " + dateVal, e);
                }

                var currentCulture = Thread.CurrentThread.CurrentCulture;
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                dateVal = parsedDate.ToString("ddd, dd MMM yyyy HH:mm:ss zzz");
                dateVal = dateVal.Remove(dateVal.LastIndexOf(':'), 1);
                Thread.CurrentThread.CurrentCulture = currentCulture;
            }

            return dateVal;
        }

        public void CheckForError()
        {
            if (this.MoveToContent() == XmlNodeType.Element)
            {
               if (this.Name != "error")
                    return;

                var message = "Error: ";

                if (this.HasAttributes)
                {
                    while (this.MoveToNextAttribute())
                    {
                        message += String.Format(" [{0}:{1}]", this.Name, this.Value);
                    }
                }

                logger.Error("Error in RSS feed: {0}", message);
                throw new Exception(message);
            }
            
        }
    }
}
