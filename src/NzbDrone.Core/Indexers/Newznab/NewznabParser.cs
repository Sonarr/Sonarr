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

        protected override ReleaseUserRatings GetUserRatings(XElement item)
        {
            var attributes = item.Elements("attr").ToList();
            var spam_reports = attributes.SingleOrDefault(e => e.Attribute("name").Value.Equals("oz_num_spam_reports", StringComparison.CurrentCultureIgnoreCase)).Attribute("value").Value;
            var spam_confirmed = attributes.SingleOrDefault(e => e.Attribute("name").Value.Equals("oz_spam_confirmed", StringComparison.CurrentCultureIgnoreCase)).Attribute("value").Value;
            var passworded_reports = attributes.SingleOrDefault(e => e.Attribute("name").Value.Equals("oz_num_passworded_reports", StringComparison.CurrentCultureIgnoreCase)).Attribute("value").Value;
            var passworded_confirmed = attributes.SingleOrDefault(e => e.Attribute("name").Value.Equals("oz_passworded_confirmed", StringComparison.CurrentCultureIgnoreCase)).Attribute("value").Value;
            var up_votes = attributes.SingleOrDefault(e => e.Attribute("name").Value.Equals("oz_up_votes", StringComparison.CurrentCultureIgnoreCase)).Attribute("value").Value;
            var down_votes = attributes.SingleOrDefault(e => e.Attribute("name").Value.Equals("oz_down_votes", StringComparison.CurrentCultureIgnoreCase)).Attribute("value").Value;
            var video_quality_rating = attributes.SingleOrDefault(e => e.Attribute("name").Value.Equals("oz_video_quality_rating", StringComparison.CurrentCultureIgnoreCase)).Attribute("value").Value;
            var audio_quality_rating = attributes.SingleOrDefault(e => e.Attribute("name").Value.Equals("oz_audio_quality_rating", StringComparison.CurrentCultureIgnoreCase)).Attribute("value").Value;
            ReleaseUserRatings userRating = new ReleaseUserRatings()
            {
                SpamReports = !string.IsNullOrWhiteSpace(spam_reports) ? Convert.ToInt32(spam_reports) + 0 : 0,
                IsSpamConfirmed = spam_confirmed == "yes",
                PasswordedReports = !string.IsNullOrWhiteSpace(passworded_reports) ? Convert.ToInt32(passworded_reports) : 0,
                IsPasswordedConfirmed = passworded_confirmed == "yes",
                UpVotes = !string.IsNullOrWhiteSpace(up_votes) && !(up_votes == "-") ? Convert.ToInt32(up_votes) : 0,
                DownVotes = !string.IsNullOrWhiteSpace(down_votes) && !(down_votes == "-") ? Convert.ToInt32(down_votes) : 0,
                VideoRating = !string.IsNullOrWhiteSpace(video_quality_rating) && !(video_quality_rating == "-") ? Convert.ToDouble(video_quality_rating) : 0,
                AudioRating = !string.IsNullOrWhiteSpace(audio_quality_rating) && !(audio_quality_rating == "-") ? Convert.ToDouble(audio_quality_rating) : 0
            };
            return userRating;
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