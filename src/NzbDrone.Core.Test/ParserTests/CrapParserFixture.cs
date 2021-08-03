using System;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class CrapParserFixture : CoreTest
    {
        [TestCase("76El6LcgLzqb426WoVFg1vVVVGx4uCYopQkfjmLe")]
        [TestCase("Vrq6e1Aba3U amCjuEgV5R2QvdsLEGYF3YQAQkw8")]
        [TestCase("TDAsqTea7k4o6iofVx3MQGuDK116FSjPobMuh8oB")]
        [TestCase("yp4nFodAAzoeoRc467HRh1mzuT17qeekmuJ3zFnL")]
        [TestCase("oxXo8S2272KE1 lfppvxo3iwEJBrBmhlQVK1gqGc")]
        [TestCase("dPBAtu681Ycy3A4NpJDH6kNVQooLxqtnsW1Umfiv")]
        [TestCase("password - \"bdc435cb-93c4-4902-97ea-ca00568c3887.337\" yEnc")]
        [TestCase("185d86a343e39f3341e35c4dad3f9959")]
        [TestCase("ba27283b17c00d01193eacc02a8ba98eeb523a76")]
        [TestCase("45a55debe3856da318cc35882ad07e43cd32fd15")]
        [TestCase("86420f8ee425340d8894bf3bc636b66404b95f18")]
        [TestCase("ce39afb7da6cf7c04eba3090f0a309f609883862")]
        [TestCase("THIS SHOULD NEVER PARSE")]
        [TestCase("Vh1FvU3bJXw6zs8EEUX4bMo5vbbMdHghxHirc.mkv")]
        [TestCase("0e895c37245186812cb08aab1529cf8ee389dd05.mkv")]
        [TestCase("08bbc153931ce3ca5fcafe1b92d3297285feb061.mkv")]
        [TestCase("185d86a343e39f3341e35c4dad3ff159")]
        [TestCase("ah63jka93jf0jh26ahjas961.mkv")]
        [TestCase("qrdSD3rYzWb7cPdVIGSn4E7")]
        [TestCase("QZC4HDl7ncmzyUj9amucWe1ddKU1oFMZDd8r0dEDUsTd")]
        [TestCase("abc.xyz.af6021c37f7852")]
        [TestCase("e096aeb3c2c0483a96f5b32fc6d10ff5")]
        [TestCase("_unpack e096aeb3c2c0483a96f5b32fc6d10ff5.mkv")]
        public void should_not_parse_crap(string title)
        {
            Parser.Parser.ParseTitle(title).Should().BeNull();
            ExceptionVerification.IgnoreWarns();
        }

        [Test]
        public void should_not_parse_md5()
        {
            string hash = "CRAPPY TEST SEED";

            var hashAlgo = System.Security.Cryptography.MD5.Create();

            var repetitions = 100;
            var success = 0;
            for (int i = 0; i < repetitions; i++)
            {
                var hashData = hashAlgo.ComputeHash(System.Text.Encoding.Default.GetBytes(hash));

                hash = BitConverter.ToString(hashData).Replace("-", "");

                if (Parser.Parser.ParseTitle(hash) == null)
                {
                    success++;
                }
            }

            success.Should().Be(repetitions);
        }

        [TestCase(32)]
        [TestCase(40)]
        public void should_not_parse_random(int length)
        {
            string charset = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

            var hashAlgo = new Random();

            var repetitions = 500;
            var success = 0;
            for (int i = 0; i < repetitions; i++)
            {
                StringBuilder hash = new StringBuilder(length);

                for (int x = 0; x < length; x++)
                {
                    hash.Append(charset[hashAlgo.Next() % charset.Length]);
                }

                if (Parser.Parser.ParseTitle(hash.ToString()) == null)
                {
                    success++;
                }
            }

            success.Should().Be(repetitions);
        }

        [TestCase("theseriestitle1618finale")]
        public void should_not_parse_file_name_without_proper_spacing(string fileName)
        {
            Parser.Parser.ParseTitle(fileName).Should().BeNull();
        }

        [TestCase("Series Title (2018) Complete 360p HDTV AAC H.264-NEXT")]
        public void should_not_parse_invalid_release_name(string fileName)
        {
            Parser.Parser.ParseTitle(fileName).Should().BeNull();
        }

        [TestCase("Specials/Series - Episode Title (part 1)")]
        public void should_not_parse_special_with_part_number(string fileName)
        {
            Parser.Parser.ParseTitle(fileName).Should().BeNull();
        }
    }
}
