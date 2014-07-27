using NzbDrone.Common;
using NzbDrone.Core.Parser.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace NzbDrone.Core.Indexers.Newznab
{
    public class OzNzbRssParser : NewznabRssParser
    {
        public const String ns_oznzb = "{http://www.oznzb.com/DTD/2014/feeds/attributes/}";

        protected override ReleaseUserRatings GetUserRatings(XElement item)
        {
            var spam_reports = TryGetOznzbAttribute(item, "oz_num_spam_reports");
            var spam_confirmed = TryGetOznzbAttribute(item, "oz_spam_confirmed");
            var passworded_reports = TryGetOznzbAttribute(item, "oz_num_passworded_reports");
            var passworded_confirmed = TryGetOznzbAttribute(item, "oz_passworded_confirmed");
            var up_votes = TryGetOznzbAttribute(item, "oz_up_votes");
            var down_votes = TryGetOznzbAttribute(item, "oz_down_votes");
            var video_quality_rating = TryGetOznzbAttribute(item, "oz_video_quality_rating");
            var audio_quality_rating = TryGetOznzbAttribute(item, "oz_audio_quality_rating");

            var userRatings = new ReleaseUserRatings();

            if (!spam_reports.IsNullOrWhiteSpace())
            {
                userRatings.SpamReports = Convert.ToInt32(spam_reports);
            }

            userRatings.IsSpamConfirmed = spam_confirmed == "yes";

            if (!passworded_reports.IsNullOrWhiteSpace())
            {
                userRatings.PasswordedReports = Convert.ToInt32(passworded_reports);
            }

            userRatings.IsPasswordedConfirmed = passworded_confirmed == "yes";
            
            if (!up_votes.IsNullOrWhiteSpace() && up_votes != "-")
            {
                userRatings.UpVotes = Convert.ToInt32(up_votes);
            }

            if (!down_votes.IsNullOrWhiteSpace() && down_votes != "-")
            {
                userRatings.DownVotes = Convert.ToInt32(down_votes);
            }
            
            if (!video_quality_rating.IsNullOrWhiteSpace() && video_quality_rating != "-")
            {
                userRatings.VideoRating = (Convert.ToDouble(video_quality_rating, CultureInfo.InvariantCulture) - 1) / 9.0;
            }

            if (!audio_quality_rating.IsNullOrWhiteSpace() && audio_quality_rating != "-")
            {
                userRatings.AudioRating = (Convert.ToDouble(audio_quality_rating, CultureInfo.InvariantCulture) - 1) / 9.0;
            }

            return userRatings;
        }

        protected String TryGetOznzbAttribute(XElement item, String key, String defaultValue = "")
        {
            var attr = item.Elements(ns_oznzb + "attr").SingleOrDefault(e => e.Attribute("name").Value.Equals(key, StringComparison.CurrentCultureIgnoreCase));

            if (attr != null)
            {
                return attr.Attribute("value").Value;
            }

            return defaultValue;
        }
    }
}
