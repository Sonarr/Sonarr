using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Integration.Test.Client;
using RestSharp;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class FileSystemFixture : IntegrationTest
    {
        public ClientBase FileSystem;

        private string _file;
        private string _folder;

        protected override void InitRestClients()
        {
            base.InitRestClients();

            FileSystem = new ClientBase(RestClient, ApiKey, "filesystem");
        }

        [SetUp]
        public void SetUp()
        {
            _file = Assembly.GetExecutingAssembly().Location;
            _folder = Path.GetDirectoryName(_file) + Path.DirectorySeparatorChar;
        }

        [Test]
        public void get_filesystem_content_excluding_files()
        {
            var request = FileSystem.BuildRequest();
            request.Method = Method.GET;
            request.AddQueryParameter("path", _folder);

            var result = FileSystem.Execute<FileSystemResult>(request, HttpStatusCode.OK);

            result.Should().NotBeNull();
            result.Directories.Should().NotBeNullOrEmpty();
            result.Files.Should().BeNullOrEmpty();
        }

        [Test]
        public void get_filesystem_content_including_files()
        {
            var request = FileSystem.BuildRequest();
            request.Method = Method.GET;
            request.AddQueryParameter("path", _folder);
            request.AddQueryParameter("includeFiles", "true");

            var result = FileSystem.Execute<FileSystemResult>(request, HttpStatusCode.OK);

            result.Should().NotBeNull();
            result.Directories.Should().NotBeNullOrEmpty();
            result.Files.Should().NotBeNullOrEmpty();

            result.Files.Should().Contain(v => v.Path == _file && v.Type == FileSystemEntityType.File);
        }

        [Test]
        public void get_entity_type_should_return_file()
        {
            var request = FileSystem.BuildRequest("type");
            request.Method = Method.GET;
            request.AddQueryParameter("path", _file);

            var result = FileSystem.Execute<FileSystemModel>(request, HttpStatusCode.OK);

            result.Type.Should().Be(FileSystemEntityType.File);
        }

        [Test]
        public void get_entity_type_should_return_folder()
        {
            var request = FileSystem.BuildRequest("type");
            request.Method = Method.GET;
            request.AddQueryParameter("path", _folder);

            var result = FileSystem.Execute<FileSystemModel>(request, HttpStatusCode.OK);

            result.Type.Should().Be(FileSystemEntityType.Folder);
        }

        [Test]
        public void get_entity_type_should_return_folder_for_unknown()
        {
            var request = FileSystem.BuildRequest("type");
            request.Method = Method.GET;
            request.AddQueryParameter("path", _file + ".unknown");

            var result = FileSystem.Execute<FileSystemModel>(request, HttpStatusCode.OK);

            result.Type.Should().Be(FileSystemEntityType.Folder);
        }

        [Test]
        public void get_all_mediafiles()
        {
            var tempDir = GetTempDirectory("mediaDir");
            File.WriteAllText(Path.Combine(tempDir, "somevideo.mkv"), "video");

            var request = FileSystem.BuildRequest("mediafiles");
            request.Method = Method.GET;
            request.AddQueryParameter("path", tempDir);

            var result = FileSystem.Execute<List<Dictionary<string, string>>>(request, HttpStatusCode.OK);

            result.Should().HaveCount(1);
            result.First().Should().ContainKey("path");
            result.First().Should().ContainKey("relativePath");
            result.First().Should().ContainKey("name");

            result.First()["name"].Should().Be("somevideo.mkv");
        }
    }
}
