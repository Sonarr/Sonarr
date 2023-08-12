namespace NzbDrone.Core.ThingiProvider.Status
{
    public static class EscalationBackOff
    {
        public static readonly int[] Periods =
        {
            0,
            60,
            5 * 60,
            15 * 60,
            30 * 60,
            60 * 60,
            3 * 60 * 60,
            6 * 60 * 60,
            12 * 60 * 60,
            24 * 60 * 60
        };
    }
}
