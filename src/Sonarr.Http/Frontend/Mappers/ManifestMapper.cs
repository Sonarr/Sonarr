using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;

namespace Sonarr.Http.Frontend.Mappers
{
    public class ManifestMapper : UrlBaseReplacementResourceMapperBase
    {
        private readonly IConfigFileProvider _configFileProvider;

        private string _generatedContent;

        public ManifestMapper(IAppFolderInfo appFolderInfo, IDiskProvider diskProvider, IConfigFileProvider configFileProvider, Logger logger)
            : base(diskProvider, configFileProvider, logger)
        {
            _configFileProvider = configFileProvider;
            FilePath = Path.Combine(appFolderInfo.StartUpFolder, configFileProvider.UiFolder, "Content", "manifest.json");
        }

        public override string Map(string resourceUrl)
        {
            return FilePath;
        }

        public override bool CanHandle(string resourceUrl)
        {
            return resourceUrl.StartsWith("/Content/manifest");
        }

        protected override string GetFileText()
        {
            if (RuntimeInfo.IsProduction && _generatedContent != null)
            {
                return _generatedContent;
            }

            var text = base.GetFileText();

            text = text.Replace("__INSTANCE_NAME__", _configFileProvider.InstanceName);

            _generatedContent = text;

            return _generatedContent;
        }
    }
}
