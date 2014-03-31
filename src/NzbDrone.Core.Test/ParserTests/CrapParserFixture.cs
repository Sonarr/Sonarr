using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Expansive;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
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
        public void should_not_parse_crap(string title)
        {
            Parser.Parser.ParseTitle(title).Should().BeNull();
            ExceptionVerification.IgnoreWarns();
        }
    }
}
