using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using NLog;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Indexers.Newznab;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers
{
    public abstract class RssParserBase : IParseFeed
    {
        protected readonly Logger _logger;

        protected virtual ReleaseInfo CreateNewReleaseInfo()
        {
            return new ReleaseInfo();
        }

        protected RssParserBase()
        {
            _logger = NzbDroneLogger.GetLogger(this);
        }

        public virtual IEnumerable<ReleaseInfo> Process(string xml, string url)
        {
            PreProcess(xml, url);

            using (var xmlTextReader = XmlReader.Create(new StringReader(xml), new XmlReaderSettings { ProhibitDtd = false, IgnoreComments = true }))
            {

                var document = XDocument.Load(xmlTextReader);
                var items = document.Descendants("item");

                var result = new List<ReleaseInfo>();

                foreach (var item in items)
                {
                    try
                    {
                        var reportInfo = ParseFeedItem(item.StripNameSpace(), url);

                        if (reportInfo != null && reportInfo.OZnzbIsPasswordedConfirmed != true && reportInfo.OZnzbIsSpamConfirmed != true) //don't add any confirmed passworded or codec spam releases 
                        {
                            reportInfo.DownloadUrl = GetNzbUrl(item);
                            reportInfo.InfoUrl = GetNzbInfoUrl(item);
                            result.Add(reportInfo);
                        }
                        
                    }
                    catch (Exception itemEx)
                    {
                        itemEx.Data.Add("Item", item.Title());
                        _logger.ErrorException("An error occurred while processing feed item from " + url, itemEx);
                    }
                }

                return result.OrderByDescending(x => x.WeightedQuality);
            }
        }

        private ReleaseInfo ParseFeedItem(XElement item, string url)
        {
            var title = GetTitle(item);

            var reportInfo = CreateNewReleaseInfo();

            reportInfo.Title = title;
            reportInfo.PublishDate = GetPublishDate(item);
            reportInfo.DownloadUrl = GetNzbUrl(item);
            reportInfo.InfoUrl = GetNzbInfoUrl(item);

            if (url.Contains("oznzb.com"))
            {
                
                reportInfo.OZnzbSpamReports = GetOZnzbSpamReports(item);
                reportInfo.OZnzbIsSpamConfirmed = GetOZnzbIsSpamConfirmed(item);
                reportInfo.OZnzbPasswordedReports = GetOZnzbPasswordedReports(item);
                reportInfo.OZnzbIsPasswordedConfirmed = GetOZnzbIsPasswordedConfirmed(item);
                reportInfo.OZnzbUpVotes = GetOZnzbUpVotes(item);
                reportInfo.OZnzbDownVotes = GetOZnzbDownVotes(item);
                reportInfo.OZnzbVideoRating = GetOZnzbVideoRating(item);
                reportInfo.OZnzbAudioRating = GetOZnzbAudioRating(item);
                reportInfo.WeightedQuality = GetWeightedQuality(reportInfo.OZnzbSpamReports + reportInfo.OZnzbPasswordedReports, reportInfo.OZnzbUpVotes, 
                                                reportInfo.OZnzbDownVotes, reportInfo.OZnzbVideoRating, reportInfo.OZnzbAudioRating, 10);
            }

            try
            {
                reportInfo.Size = GetSize(item);
            }
            catch (Exception)
            {
                throw new SizeParsingException("Unable to parse size from: {0} [{1}]", reportInfo.Title, url);
            }

            _logger.Trace("Parsed: {0}", item.Title());

            return PostProcessor(item, reportInfo);
        }

        protected virtual string GetTitle(XElement item)
        {
            return item.Title();
        }

        protected virtual DateTime GetPublishDate(XElement item)
        {
            return item.PublishDate();
        }

        protected virtual string GetNzbUrl(XElement item)
        {
            return item.Links().First();
        }

        protected virtual string GetNzbInfoUrl(XElement item)
        {
            return String.Empty;
        }

        protected abstract long GetSize(XElement item);

        protected virtual int GetOZnzbSpamReports(XElement item)
        {
            return 0;
        }
        protected virtual bool GetOZnzbIsSpamConfirmed(XElement item)
        {
            return false;
        }
        protected virtual int GetOZnzbPasswordedReports(XElement item)
        {
            return 0;
        }
        protected virtual bool GetOZnzbIsPasswordedConfirmed(XElement item)
        {
            return false;
        }
        protected virtual int GetOZnzbUpVotes(XElement item)
        {
            return 0;
        }
        protected virtual int GetOZnzbDownVotes(XElement item)
        {
            return 0;
        }
        protected virtual double GetOZnzbVideoRating(XElement item)
        {
            return 0;
        }
        protected virtual double GetOZnzbAudioRating(XElement item)
        {
            return 0;
        }
        /// <summary>
        /// Retreive a weighted quality based on user collected information such as Quality rating, up votes and reported issues.
        /// </summary>
        /// <param name="totalNegativeReports">Total number of negative reports. Use 0 if information is unavailable.</param>
        /// <param name="upVotes">Total number of up votes or thumbs up received. Use 0 if information is unavailable.</param>
        /// <param name="downVotes">Total number of down votes or thumbs down received. Use 0 if information is unavailable.</param>
        /// <param name="videoRating">Video quality rating, for example 4.5 out of 10 stars would be 4.5, 4.5 out of 5 stars would be 4.5</param>
        /// <param name="audioRating">Audio quality rating, for example 4.5 out of 10 stars would be 4.5, 4.5 out of 5 stars would be 4.5</param>
        /// <param name="highestPossibleRating">Maximum rating available in your rating system, for example 4.5 out of 10 stars would be 10, 4.5 out of 5 stars would be 5</param>
        /// <returns>Calculated weight based on user collected information</returns>
        protected virtual int GetWeightedQuality(int totalNegativeReports, int upVotes, int downVotes, double videoRating, double audioRating, int highestPossibleRating)
        {
            //Generate a weighted priority which any indexer should be able to hook into while trying to keep the numbers fair across all indexers. May require some tweaking
            int weightedPriority = 0;
            weightedPriority = totalNegativeReports * -20; //drastically reduce weight for negative reports such as virus spam or passwords.
            weightedPriority += upVotes - downVotes; //add or substact a point for every up or down vote
            if (downVotes > upVotes && downVotes >= 2)
            {
                weightedPriority += -20; // drastically reduce weight if there are more down votes than up votes and at least two down votes.
            }

            //convert any rating system to an out of 10 system, e.g. rating 3 out of 5 or 12 out of 20 all become 6 out of 10.
            int ratingMultiplier = highestPossibleRating / 10;
            int videoRatingOf10 = (int)Math.Round(videoRating * ratingMultiplier);
            int audioRatingOf10 = (int)Math.Round(audioRating * ratingMultiplier);

            //increase or decrease weight for quality rating, 4 and under are considered bad quality and are negative weighted.
            weightedPriority += (videoRatingOf10 >= 4 ? videoRatingOf10 : videoRatingOf10 - 5);
            weightedPriority += (audioRatingOf10 >= 4 ? audioRatingOf10 : videoRatingOf10 - 5);

            return weightedPriority;
        }
        protected virtual void PreProcess(string source, string url)
        {
        }

        protected virtual ReleaseInfo PostProcessor(XElement item, ReleaseInfo currentResult)
        {
            return currentResult;
        }


        private static readonly Regex ReportSizeRegex = new Regex(@"(?<value>\d+\.\d{1,2}|\d+\,\d+\.\d{1,2}|\d+)\W?(?<unit>GB|MB|GiB|MiB)",
                                                                  RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static long ParseSize(string sizeString)
        {
            var match = ReportSizeRegex.Matches(sizeString);

            if (match.Count != 0)
            {
                var cultureInfo = new CultureInfo("en-US");
                var value = Decimal.Parse(Regex.Replace(match[0].Groups["value"].Value, "\\,", ""), cultureInfo);

                var unit = match[0].Groups["unit"].Value;

                if (unit.Equals("MB", StringComparison.InvariantCultureIgnoreCase) ||
                    unit.Equals("MiB", StringComparison.InvariantCultureIgnoreCase))
                {
                    return ConvertToBytes(Convert.ToDouble(value), 2);
                }

                if (unit.Equals("GB", StringComparison.InvariantCultureIgnoreCase) ||
                        unit.Equals("GiB", StringComparison.InvariantCultureIgnoreCase))
                {
                    return ConvertToBytes(Convert.ToDouble(value), 3);
                }
            }
            return 0;
        }

        private static long ConvertToBytes(double value, int power)
        {
            var multiplier = Math.Pow(1024, power);
            var result = value * multiplier;

            return Convert.ToInt64(result);
        }
    }
}
