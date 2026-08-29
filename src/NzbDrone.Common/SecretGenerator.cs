using System.Security.Cryptography;

namespace NzbDrone.Common
{
    public static class SecretGenerator
    {
        private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";

        public static string Generate(int length)
        {
            return RandomNumberGenerator.GetString(Alphabet, length);
        }
    }
}
