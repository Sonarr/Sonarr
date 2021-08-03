using System;

namespace NzbDrone.Common.Extensions
{
    public static class LevenstheinExtensions
    {
        public static int LevenshteinDistance(this string text, string other, int costInsert = 1, int costDelete = 1, int costSubstitute = 1)
        {
            if (text == other)
            {
                return 0;
            }

            if (text.Length == 0)
            {
                return other.Length * costInsert;
            }

            if (other.Length == 0)
            {
                return text.Length * costDelete;
            }

            int[] matrix = new int[other.Length + 1];

            for (var i = 1; i < matrix.Length; i++)
            {
                matrix[i] = i * costInsert;
            }

            for (var i = 0; i < text.Length; i++)
            {
                int topLeft = matrix[0];
                matrix[0] = matrix[0] + costDelete;

                for (var j = 0; j < other.Length; j++)
                {
                    int top = matrix[j];
                    int left = matrix[j + 1];

                    var sumIns = top + costInsert;
                    var sumDel = left + costDelete;
                    var sumSub = topLeft + (text[i] == other[j] ? 0 : costSubstitute);

                    topLeft = matrix[j + 1];
                    matrix[j + 1] = Math.Min(Math.Min(sumIns, sumDel), sumSub);
                }
            }

            return matrix[other.Length];
        }

        public static int LevenshteinDistanceClean(this string expected, string other)
        {
            expected = expected.ToLower().Replace(".", "");
            other = other.ToLower().Replace(".", "");

            return expected.LevenshteinDistance(other, 1, 3, 3);
        }
    }
}
