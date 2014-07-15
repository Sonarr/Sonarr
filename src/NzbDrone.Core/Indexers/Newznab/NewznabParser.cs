using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NzbDrone.Core.Parser.Model;
using System.Globalization;

namespace NzbDrone.Core.Indexers.Newznab
{
    public class NewznabParser : RssParserBase
    {

        private static readonly string[] IgnoredErrors =
        {
            "Request limit reached",
        };

        protected override string GetNzbInfoUrl(XElement item)
        {
            return item.Comments().Replace("#comments", "");
        }

        protected override DateTime GetPublishDate(XElement item)
        {
            var attributes = item.Elements("attr").ToList();
            var usenetdateElement = attributes.SingleOrDefault(e => e.Attribute("name").Value.Equals("usenetdate", StringComparison.CurrentCultureIgnoreCase));

            if (usenetdateElement != null)
            {
                var dateString = usenetdateElement.Attribute("value").Value;

                return XElementExtensions.ParseDate(dateString);
            }

            return base.GetPublishDate(item);
        }

        protected override int GetSpamReports(XElement item)
        {
            var attributes = item.Elements("attr").ToList();
            var usenetdateElement = attributes.SingleOrDefault(e => e.Attribute("name").Value.Equals("oz_num_spam_reports", StringComparison.CurrentCultureIgnoreCase));

            if (usenetdateElement != null)
            {
                var dateString = usenetdateElement.Attribute("value").Value;
                return Convert.ToInt32(dateString);
            }

            return base.GetSpamReports(item);
        }
        protected override bool GetIsSpamConfirmed(XElement item)
        {
            var attributes = item.Elements("attr").ToList();
            var usenetdateElement = attributes.SingleOrDefault(e => e.Attribute("name").Value.Equals("oz_spam_confirmed", StringComparison.CurrentCultureIgnoreCase));

            if (usenetdateElement != null)
            {
                var dateString = usenetdateElement.Attribute("value").Value;
                return (dateString == "yes");
            }

            return base.GetIsSpamConfirmed(item);
        }
        protected override int GetPasswordedReports(XElement item)
        {
            var attributes = item.Elements("attr").ToList();
            var usenetdateElement = attributes.SingleOrDefault(e => e.Attribute("name").Value.Equals("oz_num_passworded_reports", StringComparison.CurrentCultureIgnoreCase));

            if (usenetdateElement != null)
            {
                var dateString = usenetdateElement.Attribute("value").Value;
                return Convert.ToInt32(dateString);
            }

            return base.GetPasswordedReports(item);
        }
        protected override bool GetIsPasswordedConfirmed(XElement item)
        {
            var attributes = item.Elements("attr").ToList();
            var usenetdateElement = attributes.SingleOrDefault(e => e.Attribute("name").Value.Equals("oz_passworded_confirmed", StringComparison.CurrentCultureIgnoreCase));

            if (usenetdateElement != null)
            {
                var dateString = usenetdateElement.Attribute("value").Value;
                return (dateString == "yes");
            }

            return base.GetIsPasswordedConfirmed(item);
        }
        protected override int GetUpVotes(XElement item)
        {
            var attributes = item.Elements("attr").ToList();
            var usenetdateElement = attributes.SingleOrDefault(e => e.Attribute("name").Value.Equals("oz_up_votes", StringComparison.CurrentCultureIgnoreCase));

            if (usenetdateElement != null)
            {
                var dateString = usenetdateElement.Attribute("value").Value;
                return Convert.ToInt32(dateString);
            }

            return base.GetUpVotes(item);
        }
        protected override int GetDownVotes(XElement item)
        {
            var attributes = item.Elements("attr").ToList();
            var usenetdateElement = attributes.SingleOrDefault(e => e.Attribute("name").Value.Equals("oz_down_votes", StringComparison.CurrentCultureIgnoreCase));

            if (usenetdateElement != null)
            {
                var dateString = usenetdateElement.Attribute("value").Value;
                return Convert.ToInt32(dateString);
            }

            return base.GetDownVotes(item);
        }
        protected override double GetVideoRating(XElement item)
        {
            var attributes = item.Elements("attr").ToList();
            var usenetdateElement = attributes.SingleOrDefault(e => e.Attribute("name").Value.Equals("oz_video_quality_rating", StringComparison.CurrentCultureIgnoreCase));

            if (usenetdateElement != null)
            {
                var dateString = usenetdateElement.Attribute("value").Value;
                return !(dateString == "-") ? Convert.ToDouble(dateString) : 0;
            }

            return base.GetVideoRating(item);
        }
        protected override double GetAudioRating(XElement item)
        {
            var attributes = item.Elements("attr").ToList();
            var usenetdateElement = attributes.SingleOrDefault(e => e.Attribute("name").Value.Equals("oz_audio_quality_rating", StringComparison.CurrentCultureIgnoreCase));

            if (usenetdateElement != null)
            {
                var dateString = usenetdateElement.Attribute("value").Value;
                return !(dateString == "-") ? Convert.ToDouble(dateString) : 0;
            }

            return base.GetAudioRating(item);
        }
        protected override long GetSize(XElement item)
        {
            var attributes = item.Elements("attr").ToList();
            var sizeElement = attributes.SingleOrDefault(e => e.Attribute("name").Value.Equals("size", StringComparison.CurrentCultureIgnoreCase));

            if (sizeElement != null)
            {
                return Convert.ToInt64(sizeElement.Attribute("value").Value);
            }

            return ParseSize(item.Description());
        }

        public override IEnumerable<ReleaseInfo> Process(string xml, string url)
        {
            try
            {
                return base.Process(xml, url);
            }
            catch (NewznabException e)
            {
                if (!IgnoredErrors.Any(ignoredError => e.Message.Contains(ignoredError)))
                {
                    throw;
                }
                _logger.Error(e.Message);
                return new List<ReleaseInfo>();
            }
        }

        protected override ReleaseInfo PostProcessor(XElement item, ReleaseInfo currentResult)
        {
            if (currentResult != null)
            {
                var attributes = item.Elements("attr").ToList();

                var rageIdElement = attributes.SingleOrDefault(e => e.Attribute("name").Value.Equals("rageid", StringComparison.CurrentCultureIgnoreCase));

                if (rageIdElement != null)
                {
                    int tvRageId;

                    if (Int32.TryParse(rageIdElement.Attribute("value").Value, out tvRageId))
                    {
                        currentResult.TvRageId = tvRageId;
                    }
                }
            }

            return currentResult;
        }

        protected override void PreProcess(string source, string url)
        {
            NewznabPreProcessor.Process(source, url);
        }
    }
}