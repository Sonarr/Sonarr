namespace NzbDrone.Common
{
    public static class ConvertBase32
    {
        private static string ValidChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        public static byte[] FromBase32String(string str)
        {
            int numBytes = str.Length * 5 / 8;
            byte[] bytes = new byte[numBytes];

            // all UPPERCASE chars
            str = str.ToUpper();

            int bitBuffer = 0;
            int bitBufferCount = 0;
            int index = 0;

            for (int i = 0; i < str.Length; i++)
            {
                bitBuffer = (bitBuffer << 5) | ValidChars.IndexOf(str[i]);
                bitBufferCount += 5;

                if (bitBufferCount >= 8)
                {
                    bitBufferCount -= 8;
                    bytes[index++] = (byte)(bitBuffer >> bitBufferCount);
                }
            }

            return bytes;
        }
    }
}
