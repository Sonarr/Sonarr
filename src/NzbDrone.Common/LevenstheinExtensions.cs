using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Zip;

namespace NzbDrone.Common
{
    public static class LevenstheinExtensions
    {
        public static Int32 LevenshteinDistance(this String text, String other, Int32 costInsert = 1, Int32 costDelete = 1, Int32 costSubstitute = 1)
        {
            if (text == other) return 0;
            if (text.Length == 0) return other.Length * costInsert;
            if (other.Length == 0) return text.Length * costDelete;

            Int32[] matrix = new Int32[other.Length + 1];

            for (var i = 1; i < matrix.Length; i++)
            {
                matrix[i] = i * costInsert;
            }

            for (var i = 0; i < text.Length; i++)
            {
                Int32 topLeft = matrix[0];
                matrix[0] = matrix[0] + costDelete;

                for (var j = 0; j < other.Length; j++)
                {
                    Int32 top = matrix[j];
                    Int32 left = matrix[j + 1];

                    var sumIns = top + costInsert;
                    var sumDel = left + costDelete;
                    var sumSub = topLeft + (text[i] == other[j] ? 0 : costSubstitute);

                    topLeft = matrix[j + 1];
                    matrix[j + 1] = Math.Min(Math.Min(sumIns, sumDel), sumSub);
                }
            }

            return matrix[other.Length];
        }

        public static Int32 LevenshteinDistanceClean(this String expected, String other)
        {
            expected = expected.ToLower().Replace(".", "");
            other = other.ToLower().Replace(".", "");

            return expected.LevenshteinDistance(other, 1, 3, 3);
        }
    }
}