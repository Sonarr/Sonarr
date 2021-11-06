using System;
using System.IO;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;

namespace Sonarr.Http.Frontend.Mappers
{
    public abstract class HtmlMapperBase : StaticResourceMapperBase
    {
        private readonly IDiskProvider _diskProvider;
        private readonly Lazy<ICacheBreakerProvider> _cacheBreakProviderFactory;
        private static readonly Regex ReplaceRegex = new Regex(@"(?:(?<attribute>href|src)=\"")(?<path>.*?(?<extension>css|js|png|ico|ics|svg|json))(?:\"")(?:\s(?<nohash>data-no-hash))?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private string _generatedContent;

        protected HtmlMapperBase(IDiskProvider diskProvider,
                                 Lazy<ICacheBreakerProvider> cacheBreakProviderFactory,
                                 Logger logger)
            : base(diskProvider, logger)
        {
            _diskProvider = diskProvider;
            _cacheBreakProviderFactory = cacheBreakProviderFactory;
        }

        protected string HtmlPath;
        protected string UrlBase;

        protected override Stream GetContentStream(string filePath)
        {
            var text = GetHtmlText();

            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(text);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        protected string GetHtmlText()
        {
            if (RuntimeInfo.IsProduction && _generatedContent != null)
            {
                return _generatedContent;
            }

            var text = _diskProvider.ReadAllText(HtmlPath);
            var cacheBreakProvider = _cacheBreakProviderFactory.Value;

            text = ReplaceRegex.Replace(text, match =>
            {
                string url;

                if (match.Groups["nohash"].Success)
                {
                    url = match.Groups["path"].Value;
                }
                else
                {
                    url = cacheBreakProvider.AddCacheBreakerToPath(match.Groups["path"].Value);
                }

                return string.Format("{0}=\"{1}{2}\"", match.Groups["attribute"].Value, UrlBase, url);
            });

            _generatedContent = text;

            return _generatedContent;
        }
    }
}
