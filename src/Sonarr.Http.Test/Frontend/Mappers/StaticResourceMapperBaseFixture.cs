using System.IO;
using FluentAssertions;
using NLog;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Test.Common;
using Sonarr.Http.Frontend.Mappers;

namespace Sonarr.Http.Test.Frontend.Mappers
{
    [TestFixture]
    public class StaticResourceMapperBaseFixture : TestBase
    {
        private string _folder;
        private TestMapper _subject;

        [SetUp]
        public void Setup()
        {
            _folder = Path.Combine(TempFolder, "static");

            _subject = new TestMapper(_folder, Mocker.Resolve<IDiskProvider>(), TestLogger);
        }

        [Test]
        public void should_return_path_when_resolved_path_is_inside_folder()
        {
            _subject.MapPathResult = Path.Combine(_folder, "index.html");

            _subject.Map("/index.html").Should().Be(Path.Combine(_folder, "index.html"));
        }

        [Test]
        public void should_return_null_when_resolved_path_traverses_outside_folder()
        {
            _subject.MapPathResult = Path.Combine(_folder, "..", "secret.txt");

            _subject.Map("/../secret.txt").Should().BeNull();
        }

        [Test]
        public void should_return_null_when_resolved_path_is_sibling_with_matching_prefix()
        {
            var sibling = _folder + "-other";
            _subject.MapPathResult = Path.Combine(sibling, "index.html");

            _subject.Map("/index.html").Should().BeNull();
        }

        private class TestMapper : StaticResourceMapperBase
        {
            private readonly string _folderPath;

            public string MapPathResult { get; set; }

            public TestMapper(string folderPath, IDiskProvider diskProvider, Logger logger)
                : base(diskProvider, logger)
            {
                _folderPath = folderPath;
            }

            protected override string FolderPath => _folderPath;

            protected override string MapPath(string resourceUrl) => MapPathResult;

            public override bool CanHandle(string resourceUrl) => true;
        }
    }
}
