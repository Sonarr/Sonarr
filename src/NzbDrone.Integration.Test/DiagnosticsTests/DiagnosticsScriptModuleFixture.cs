using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Diagnostics;
using NzbDrone.Integration.Test.Client;
using RestSharp;
using Sonarr.Api.V3.Diagnostics;

namespace NzbDrone.Integration.Test.DiagnosticsTests
{
    public class DiagnosticScriptResource
    {
        public string Code { get; set; }
        public bool? Debug { get; set; }

        public object ReturnValue { get; set; }
        public Dictionary<string, object> DebugVariables { get; set; }

        public string Error { get; set; }
        public List<ScriptDiagnostic> ErrorDiagnostics { get; set; }
    }

    public class DiagnosticsScriptClient : ClientBase
    {
        public DiagnosticsScriptClient(IRestClient restClient, string apiKey)
            : base(restClient, apiKey, "v3/diagnostic/script")
        {
        }

        public DiagnosticScriptResource Execute(DiagnosticScriptResource body, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var request = BuildRequest("execute");
            request.Method = Method.POST;
            request.AddJsonBody(body);
            return Execute<DiagnosticScriptResource>(request, statusCode);
        }
    }

    public class DiagnosticsScriptModuleFixture : IntegrationTest
    {
        DiagnosticsScriptClient DiagScript { get; set; }

        [SetUp]
        public void SetUp()
        {
            DiagScript = new DiagnosticsScriptClient(RestClient, ApiKey);
        }

        private void GivenEnabledFeature(bool enabled = true)
        {
            var debugscripts = Path.Combine(_runner.AppData, "debugscripts");

            if (enabled && !Directory.Exists(debugscripts))
            {
                Directory.CreateDirectory(debugscripts);
            }
            else if (!enabled && Directory.Exists(debugscripts))
            {
                Directory.Delete(debugscripts);
            }
        }

        [Test]
        public void should_not_allow_access_without_debugscripts_dir()
        {
            GivenEnabledFeature(false);

            DiagScript.Execute(new DiagnosticScriptResource
            {
                Code = "return \"abc\";"
            }, HttpStatusCode.NotFound);
        }

        [Test]
        public void should_not_allow_access_with_debugscripts_dir()
        {
            GivenEnabledFeature(true);

            var result = DiagScript.Execute(new DiagnosticScriptResource
            {
                Code = "return \"abc\";"
            });

            result.ReturnValue.Should().Be("abc");
        }

        [Test]
        public void should_include_variables_for_debug()
        {
            GivenEnabledFeature();

            var result = DiagScript.Execute(new DiagnosticScriptResource
            {
                Code = "var a = \"abc\";",
                Debug = true
            });

            result.ReturnValue.Should().BeNull();
            result.DebugVariables.Should().Contain("a", "abc");
        }

        [Test]
        public void should_not_include_variables_without_debug()
        {
            GivenEnabledFeature();

            var result = DiagScript.Execute(new DiagnosticScriptResource
            {
                Code = "var a = \"abc\";"
            });

            result.ReturnValue.Should().BeNull();
            result.DebugVariables.Should().BeNull();
        }

        [Test]
        public void should_report_compile_errors_with_debug()
        {
            GivenEnabledFeature();

            var result = DiagScript.Execute(new DiagnosticScriptResource
            {
                Code = "var a = \"abc\" + b;",
                Debug = true
            });

            result.ReturnValue.Should().BeNull();
            result.DebugVariables.Should().BeNull();
            result.Error.Should().Be("ScriptConsole.cs(1,17): error CS0103: The name 'b' does not exist in the current context");
            result.ErrorDiagnostics.Should().NotBeNull();
            result.ErrorDiagnostics.First().Message.Should().Be("The name 'b' does not exist in the current context");
        }

        [Test]
        public void should_report_compile_errors_without_debug()
        {
            GivenEnabledFeature();

            var result = DiagScript.Execute(new DiagnosticScriptResource
            {
                Code = "var a = \"abc\" + b;"
            });

            result.ReturnValue.Should().BeNull();
            result.DebugVariables.Should().BeNull();
            result.Error.Should().Be("(1,17): error CS0103: The name 'b' does not exist in the current context");
            result.ErrorDiagnostics.Should().NotBeNull();
            result.ErrorDiagnostics.First().Message.Should().Be("The name 'b' does not exist in the current context");
        }

        [Test]
        public void should_report_execution_errors_with_debug()
        {
            GivenEnabledFeature();

            var result = DiagScript.Execute(new DiagnosticScriptResource
            {
                Code = "var seriesService = Resolve<ISeriesService>();\nseriesService.AddSeries((Series)null);",
                Debug = true
            });

            result.ReturnValue.Should().BeNull();
            result.DebugVariables.Should().BeNull();
            result.Error.Should().StartWith("Object reference not set to an instance of an object");
            result.ErrorDiagnostics.Should().NotBeNull();
            result.ErrorDiagnostics.First().StartLineNumber.Should().Be(2);
            result.ErrorDiagnostics.First().StartColumn.Should().Be(1);
            result.ErrorDiagnostics.First().Message.Should().StartWith("Object reference not set to an instance of an object");
            result.ErrorDiagnostics.First().FullMessage.Should().StartWith("System.NullReferenceException: Object reference not set to an instance of an object");
        }

        [Test]
        public void should_report_execution_errors_without_debug()
        {
            GivenEnabledFeature();

            var result = DiagScript.Execute(new DiagnosticScriptResource
            {
                Code = "var seriesService = Resolve<ISeriesService>();\nseriesService.AddSeries((Series)null);",
            });

            result.ReturnValue.Should().BeNull();
            result.DebugVariables.Should().BeNull();
            result.Error.Should().StartWith("Object reference not set to an instance of an object");
            result.ErrorDiagnostics.Should().NotBeNull();
            result.ErrorDiagnostics.First().StartLineNumber.Should().Be(1);
            result.ErrorDiagnostics.First().EndLineNumber.Should().Be(2);
            result.ErrorDiagnostics.First().EndColumn.Should().Be(38);
            result.ErrorDiagnostics.First().Message.Should().StartWith("Object reference not set to an instance of an object");
            result.ErrorDiagnostics.First().FullMessage.Should().StartWith("System.NullReferenceException: Object reference not set to an instance of an object");
        }
    }
}
