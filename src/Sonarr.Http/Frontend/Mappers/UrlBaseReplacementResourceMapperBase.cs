using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;

namespace Sonarr.Http.Frontend.Mappers
{
    public abstract class UrlBaseReplacementResourceMapperBase : StaticResourceMapperBase
    {
        private readonly IDiskProvider _diskProvider;
        private readonly string _urlBase;

        private string _generatedContent;

        public UrlBaseReplacementResourceMapperBase(IDiskProvider diskProvider, IConfigFileProvider configFileProvider, Logger logger)
            : base(diskProvider, logger)
        {
            _diskProvider = diskProvider;
            _urlBase = configFileProvider.UrlBase;
        }

        protected string FilePath;

        public override string Map(string resourceUrl)
        {
            return FilePath;
        }

        protected override Stream GetContentStream(string filePath)
        {
            var text = GetFileText();

            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(text);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        protected virtual string GetFileText()
        {
            if (RuntimeInfo.IsProduction && _generatedContent != null)
            {
                return _generatedContent;
            }

            var text = _diskProvider.ReadAllText(FilePath);

            text = text.Replace("__URL_BASE__", _urlBase);

            _generatedContent = text;

            return _generatedContent;
        }
    }
}
