namespace NzbDrone.Common.EnsureThat
{
    internal static class StringExtensions
    {
        internal static string Inject(this string format, params object[] formattingArgs)
        {
            return string.Format(format, formattingArgs);
        }

        internal static string Inject(this string format, params string[] formattingArgs)
        {
            return string.Format(format, formattingArgs);
        }
    }
}