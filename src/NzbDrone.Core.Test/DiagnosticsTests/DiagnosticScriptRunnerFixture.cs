using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Composition;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Diagnostics;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.DiagnosticsTests
{
    public class DiagnosticScriptRunnerFixture : CoreTest<DiagnosticScriptRunner>
    {
        [TestCase("1 ++ 2", "(1,6): error CS1002: ; expected")]
        [TestCase("a = 2", "(1,1): error CS0103: The name 'a' does not exist in the current context")]
        [TestCase("Logger.NoMethod()", "(1,8): error CS1061: 'Logger' does not contain a definition for 'NoMethod' and no accessible extension method 'NoMethod' accepting a first argument of type 'Logger' could be found (are you missing a using directive or an assembly reference?)")]
        public void Validate_should_list_compiler_errors(string source, string message)
        {
            var result = Subject.Validate(new ScriptRequest { Code = source, Debug = true });

            result.HasErrors.Should().BeTrue();
            result.Messages.First().FullMessage.Should().Be("ScriptConsole.cs" + message);
        }

        [Test]
        public void Execute_should_show_context()
        {
            var result = Subject.Execute(new ScriptRequest { Code = "var a = 12;" });

            result.ReturnValue.Should().BeNull();
            result.Variables.Should().HaveCount(1);
            result.Variables["a"].Should().Be(12);
        }

        [Test]
        public void Execute_should_allow_continuations()
        {
            var result = Subject.Execute(new ScriptRequest { Code = "var a = 12;", StoreState = true });

            var result2 = Subject.Execute(new ScriptRequest { Code = "var b = a + 2;", StateId = result.StateId });

            result2.Variables.Should().HaveCount(2);
            result2.Variables["b"].Should().Be(14);
        }

        [Test]
        public void Execute_should_resolve_interfaces_Common()
        {
            Mocker.SetConstant<IContainer>(new AutoMoqerContainer(Mocker));

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.FolderExists("C:\test"))
                .Returns(true);

            var result = Subject.Execute(new ScriptRequest { Code = @"
            var diskProvider = Resolve<IDiskProvider>();

            return diskProvider.FolderExists(""C:\test"") ? ""yes"" : ""no"";
            " });

            result.ReturnValue.Should().Be("yes");
        }

        [Test]
        public void Execute_should_resolve_interfaces_Core()
        {
            Mocker.SetConstant<IContainer>(new AutoMoqerContainer(Mocker));

            Mocker.GetMock<ISeriesService>()
                .Setup(v => v.GetAllSeries())
                .Returns(Builder<Series>.CreateListOfSize(5).BuildList());

            var result = Subject.Execute(new ScriptRequest { Code = @"
            var seriesService = Resolve<ISeriesService>();

            foreach (var series in seriesService.GetAllSeries())
            {
                await Task.Delay(1000);
                Logger.Debug($""Processing series {series.Title}"");
            }
            return ""done"";
            " });

            result.ReturnValue.Should().Be("done");
        }
    }
}
