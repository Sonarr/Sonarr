namespace NzbDrone.Common.Extensions
{
    public static class NullableExtensions
    {
        public static int? NonNegative(this int? source)
        {
            if (source.HasValue && source.Value != -1)
            {
                return source;
            }

            return null;
        }
    }
}
