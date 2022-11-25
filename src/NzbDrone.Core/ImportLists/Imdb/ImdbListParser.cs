using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.ImportLists.Exceptions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.ImportLists.Imdb
{
    public class ImdbListParser : IParseImportListResponse
    {
        public IList<ImportListItemInfo> ParseResponse(ImportListResponse importListResponse)
        {
            var importResponse = importListResponse;

            var series = new List<ImportListItemInfo>();

            if (!PreProcess(importResponse))
            {
                return series;
            }

            // Parse TSV response from IMDB export
            var rows = importResponse.Content.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            series = rows.Skip(1).SelectList(m => m.Split(',')).Where(m => m.Length > 1).SelectList(i => new ImportListItemInfo { ImdbId = i[1] });

            return series;
        }

        protected virtual bool PreProcess(ImportListResponse listResponse)
        {
            if (listResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new ImportListException(listResponse,
                    "Imdb call resulted in an unexpected StatusCode [{0}]",
                    listResponse.HttpResponse.StatusCode);
            }

            if (listResponse.HttpResponse.Headers.ContentType != null &&
                listResponse.HttpResponse.Headers.ContentType.Contains("text/json") &&
                listResponse.HttpRequest.Headers.Accept != null &&
                !listResponse.HttpRequest.Headers.Accept.Contains("text/json"))
            {
                throw new ImportListException(listResponse,
                    "Imdb responded with html content. Site is likely blocked or unavailable.");
            }

            return true;
        }
    }
}
