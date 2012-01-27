using System;

namespace NzbDrone.Core.Helpers
{
    public static class FileSizeFormatHelper
    {
        private const Decimal OneKiloByte = 1024M;
        private const Decimal OneMegaByte = OneKiloByte * 1024M;
        private const Decimal OneGigaByte = OneMegaByte * 1024M;

        public static string Format(long bytes, int precision = 0)
        {
            if (bytes == 0)
                return "0B";

            decimal size = Convert.ToDecimal(bytes);

            string suffix;

            if (size > OneGigaByte)
            {
                size /= OneGigaByte;
                suffix = "GB";
            }

            else if (size > OneMegaByte)
            {
                size /= OneMegaByte;
                suffix = "MB";
            }

            else if (size > OneKiloByte)
            {
                size /= OneKiloByte;
                suffix = "KB";
            }

            else
            {
                suffix = " B";
            }

            return String.Format("{0:N" + precision + "}{1}", size, suffix);
        }
    }
}
