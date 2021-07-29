namespace NzbDrone.Common.OAuth
{
    /// <summary>
    /// The encryption method to use when hashing a request signature.
    /// </summary>
    public enum OAuthSignatureMethod
    {
        HmacSha1,
        PlainText,
        RsaSha1
    }
}
