using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Common.Model;
using NzbDrone.Test.Common;
using NzbDrone.Update.Providers;

namespace NzbDrone.Update.Test
{
    [TestFixture]
    public class ProgramFixture : TestBase
    {
        private Program _program;

        [SetUp]
        public void Setup()
        {
            _program = Mocker.Resolve<Program>();
        }

        [Test]
        public void should_throw_if_null_passed_in()
        {
            Assert.Throws<ArgumentException>(() => _program.Start(null));
        }

        [Test]
        public void should_throw_if_less_than_two_arguments_arent_passed_in()
        {
            Assert.Throws<ArgumentException>(() => _program.Start(new[] { "" }));
        }

        [Test]
        public void should_throw_if_more_than_two_arguments_arent_passed_in()
        {
            Assert.Throws<ArgumentException>(() => _program.Start(new[] { "", "", "" }));
        }

        [TestCase("d", "")]
        [TestCase("", "")]
        [TestCase("0", "")]
        [TestCase("-1", "")]
        [TestCase(" ", "")]
        [TestCase(".", "")]
        public void should_throw_if_first_arg_isnt_an_int(string arg1, string arg2)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _program.Start(new[] { arg1, arg2 }));
        }

        [Test]
        public void should_call_update_with_corret_path()
        {
            const string ProcessPath = @"C:\NzbDrone\nzbdrone.exe";

            Mocker.GetMock<IProcessProvider>().Setup(c => c.GetProcessById(12))
                .Returns(new ProcessInfo() { StartPath = ProcessPath });

            
            _program.Start(new[] { "12", "" });

            
            Mocker.GetMock<UpdateProvider>().Verify(c => c.Start(@"C:\NzbDrone"), Times.Once());
        }


    }
}
