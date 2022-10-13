namespace NzbDrone.Core.Download.Clients.FreeboxDownload
{
    public static class EncodingForBase64
    {
        public static string EncodeBase64(this string text)
        {
            if (text == null)
            {
                return null;
            }

            byte[] textAsBytes = System.Text.Encoding.UTF8.GetBytes(text);
            return System.Convert.ToBase64String(textAsBytes);
        }

        public static string DecodeBase64(this string encodedText)
        {
            if (encodedText == null)
            {
                return null;
            }

            byte[] textAsBytes = System.Convert.FromBase64String(encodedText);
            return System.Text.Encoding.UTF8.GetString(textAsBytes);
        }
    }
}
