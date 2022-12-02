using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.CustomFormats
{
    public class CustomFormatsTestHelpers : CoreTest
    {
        private static List<CustomFormat> _customFormats { get; set; }

        public static void GivenCustomFormats(params CustomFormat[] formats)
        {
            _customFormats = formats.ToList();
        }

        public static List<ProfileFormatItem> GetSampleFormatItems(params string[] allowed)
        {
            var allowedItems = _customFormats.Where(x => allowed.Contains(x.Name)).Select((f, index) => new ProfileFormatItem { Format = f, Score = (int)Math.Pow(2, index) }).ToList();
            var disallowedItems = _customFormats.Where(x => !allowed.Contains(x.Name)).Select(f => new ProfileFormatItem { Format = f, Score = -1 * (int)Math.Pow(2, allowedItems.Count) });

            return disallowedItems.Concat(allowedItems).ToList();
        }

        public static List<ProfileFormatItem> GetDefaultFormatItems()
        {
            return new List<ProfileFormatItem>();
        }
    }
}
