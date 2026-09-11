using System;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Test.Common;
using Sonarr.Http.Frontend;

namespace Sonarr.Http.Test.Frontend
{
    [TestFixture]
    public class ViteDevServerFixture : TestBase
    {
        private const string EnvironmentVariable = "SONARR_VITE_DEV_SERVER";

        private string _previousAddress;

        [SetUp]
        public void Setup()
        {
            _previousAddress = Environment.GetEnvironmentVariable(EnvironmentVariable);
            Environment.SetEnvironmentVariable(EnvironmentVariable, null);
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable(EnvironmentVariable, _previousAddress);
        }

        [TestCase("/@vite/client")]
        [TestCase("/@react-refresh")]
        [TestCase("/@id/virtual:module")]
        [TestCase("/@fs/Users/dev/project/file.ts")]
        [TestCase("/node_modules/.vite/deps/react.js")]
        [TestCase("/frontend/src/index.ts")]
        [TestCase("/frontend/src/Components/Page/Page.module.css")]
        public void should_handle_vite_dev_server_paths(string resourceUrl)
        {
            ViteDevServer.IsViteDevPath(resourceUrl).Should().BeTrue();
        }

        [TestCase("/api/v3/series")]
        [TestCase("/Content/Fonts/fonts.css")]
        [TestCase("/assets/index-abc123.js")]
        [TestCase("/login")]
        [TestCase("/")]
        [TestCase("/series/1")]
        public void should_not_handle_application_paths(string resourceUrl)
        {
            ViteDevServer.IsViteDevPath(resourceUrl).Should().BeFalse();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void should_not_be_enabled_without_a_dev_server_address(string baseAddress)
        {
            new ViteDevServer(baseAddress).IsEnabled.Should().BeFalse();
        }

        [Test]
        public void should_be_enabled_with_a_dev_server_address()
        {
            new DebugViteDevServer("http://localhost:8959").IsEnabled.Should().BeTrue();
        }

        [Test]
        public void should_only_be_enabled_for_debug_builds()
        {
            new ViteDevServer("http://localhost:8959").IsEnabled.Should().Be(BuildInfo.IsDebug);
        }

        [TestCase("/@vite/client")]
        [TestCase("/@react-refresh")]
        [TestCase("/@id/virtual:module")]
        [TestCase("/@fs/Users/dev/project/file.ts")]
        [TestCase("/node_modules/.vite/deps/react.js")]
        [TestCase("/frontend/src/index.ts")]
        [TestCase("/frontend/src/Components/Page/Page.module.css")]
        public void should_not_handle_any_path_when_disabled(string resourceUrl)
        {
            new DebugViteDevServer(null).HandlesPath(resourceUrl).Should().BeFalse();
        }

        [TestCase("/@vite/client")]
        [TestCase("/@react-refresh")]
        [TestCase("/@id/virtual:module")]
        [TestCase("/@fs/Users/dev/project/file.ts")]
        [TestCase("/node_modules/.vite/deps/react.js")]
        [TestCase("/frontend/src/index.ts")]
        [TestCase("/frontend/src/Components/Page/Page.module.css")]
        public void should_handle_vite_dev_server_paths_when_enabled(string resourceUrl)
        {
            new DebugViteDevServer("http://localhost:8959").HandlesPath(resourceUrl).Should().BeTrue();
        }

        [TestCase("/api/v3/series")]
        [TestCase("/Content/Fonts/fonts.css")]
        [TestCase("/assets/index-abc123.js")]
        public void should_not_handle_application_paths_when_enabled(string resourceUrl)
        {
            new DebugViteDevServer("http://localhost:8959").HandlesPath(resourceUrl).Should().BeFalse();
        }

        private class DebugViteDevServer : ViteDevServer
        {
            public DebugViteDevServer(string baseAddress)
                : base(baseAddress)
            {
            }

            protected override bool IsDebugBuild => true;
        }
    }
}
