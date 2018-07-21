using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace NzbDrone.Core.Indexers.YggTorrent
{
    public static class FriendlyUnitConverter
    {
        public enum SizeUnits
        {
            Unknown = -1,

            // FR
            Octet = 0,
            o = 0,
            Ko = 1,
            KiloOctet = 1,
            Mo = 2,
            MegaOctet = 2,
            Go = 3,
            GigaOctet = 3,
            To = 4,
            TeraOctet = 4,

            // EN
            Byte = 0,
            B = 0,
            KB = 1,
            KiloBytes = 1,
            MB = 2,
            MegaBytes = 3,
            GB = 3,
            GigaBytes = 3,
            TB = 4,
            TeraBytes = 4
        }

        public static double ToByte(this string value)
        {
            Regex regex = new Regex(@"^(\d{1,}(?:.\d{1,})?)([ a-zA-Z]{1,})$");

            Match match = regex.Match(value);

            if (match.Success)
            {
                double size = Convert.ToDouble(match.Groups[1].Value, CultureInfo.InvariantCulture);
                string unitStr = match.Groups[2].Value.Trim();

                SizeUnits unit = SizeUnits.Unknown;
                if (!Enum.TryParse(unitStr, true, out unit))
                {
                    throw new ArgumentOutOfRangeException(unitStr, "Invalid SizeUnit to convert to.");
                }

                return (size * (double)Math.Pow(1024, (int)unit));
            }

            throw new Exception("Data cannot be converted.");
        }
    }
}