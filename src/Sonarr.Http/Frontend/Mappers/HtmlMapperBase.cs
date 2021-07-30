using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Nancy;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;

namespace Sonarr.Http.Frontend.Mappers
{
    public abstract class HtmlMapperBase : StaticResourceMapperBase
    {
        private readonly IDiskProvider _diskProvider;
        private readonly Func<ICacheBreakerProvider> _cacheBreakProviderFactory;
        private static readonly Regex ReplaceRegex = new Regex(@"(?:(?<attribute>href|src)=\"")(?<path>.*?(?<extension>css|js|png|ico|ics|svg|json))(?:\"")(?:\s(?<nohash>data-no-hash))?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private string _generatedContent;

        protected HtmlMapperBase(IDiskProvider diskProvider,
                                 Func<ICacheBreakerProvider> cacheBreakProviderFactory,
                                 Logger logger)
            : base(diskProvider, logger)
        {
            _diskProvider = diskProvider;
            _cacheBreakProviderFactory = cacheBreakProviderFactory;
        }

        protected string HtmlPath;
        protected string UrlBase;

        protected override Task<byte[]> GetContent(string filePath)
        {
            var text = GetHtmlText();
            var data = Encoding.UTF8.GetBytes(text);
            return Task.FromResult(data);
        }

        public async override Task<Response> GetResponse(string resourceUrl)
        {
            var response = await base.GetResponse(resourceUrl);
            response.Headers["X-UA-Compatible"] = "IE=edge";

            return response;
        }

        protected string GetHtmlText()
        {
            if (RuntimeInfo.IsProduction && _generatedContent != null)
            {
                return _generatedContent;
            }

            var text = _diskProvider.ReadAllText(HtmlPath);
            var cacheBreakProvider = _cacheBreakProviderFactory();

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
