namespace NzbDrone.Common.OAuth
{
    /// <summary>
    /// Specifies whether the final signature value should be escaped during calculation.
    /// This might be necessary for some OAuth implementations that do not obey the default
    /// specification for signature escaping.
    /// </summary>
    public enum OAuthSignatureTreatment
    {
        Escaped,
        Unescaped
    }
}
