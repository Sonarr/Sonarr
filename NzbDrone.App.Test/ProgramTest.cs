using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Model;
using NzbDrone.Providers;

namespace NzbDrone.App.Test
{
    [TestFixture]
    public class ProgramTest
    {

        [TestCase(null, ApplicationMode.Console)]
        [TestCase("", ApplicationMode.Console)]
        [TestCase("1", ApplicationMode.Help)]
        [TestCase("ii", ApplicationMode.Help)]
        [TestCase("uu", ApplicationMode.Help)]
        [TestCase("i", ApplicationMode.InstallService)]
        [TestCase("I", ApplicationMode.InstallService)]
        [TestCase("/I", ApplicationMode.InstallService)]
        [TestCase("/i", ApplicationMode.InstallService)]
        [TestCase("-I", ApplicationMode.InstallService)]
        [TestCase("-i", ApplicationMode.InstallService)]
        [TestCase("u", ApplicationMode.UninstallService)]
        [TestCase("U", ApplicationMode.UninstallService)]
        [TestCase("/U", ApplicationMode.UninstallService)]
        [TestCase("/u", ApplicationMode.UninstallService)]
        [TestCase("-U", ApplicationMode.UninstallService)]
        [TestCase("-u", ApplicationMode.UninstallService)]
        public void GetApplicationMode_single_arg(string arg, ApplicationMode mode)
        {
            Console.GetApplicationMode(new[] { arg }).Should().Be(mode);
        }

        [TestCase("", "", ApplicationMode.Console)]
        [TestCase("", null, ApplicationMode.Console)]
        [TestCase("i", "n", ApplicationMode.Help)]
        public void GetApplicationMode_two_args(string a, string b, ApplicationMode mode)
        {
            Console.GetApplicationMode(new[] { a, b }).Should().Be(mode);
        }
    }
}
