using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Tv
{
    public static class SeriesTitleNormalizer
    {
        private readonly static Dictionary<Int32, String> PreComputedTitles = new Dictionary<Int32, String>
                                                                     {
                                                                         { 281588, "a to z" },
                                                                         { 266757, "ad trials triumph early church" }
                                                                         { 289260, "ad bible continues"}
                                                                     };

        public static String Normalize(String title, Int32 tvdbId)
        {
            if (PreComputedTitles.ContainsKey(tvdbId))
            {
                return PreComputedTitles[tvdbId];
            }

            return Parser.Parser.NormalizeTitle(title).ToLower();
        }
    }
}
