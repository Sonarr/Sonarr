using System;
using System.Collections.Generic;
using System.Net;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.ImportLists.Exceptions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.ImportLists.Tmdb;

public abstract class TmdbParserBase<TResponse> : IParseImportListResponse
    where TResponse : new()
{
    public IList<ImportListItemInfo> ParseResponse(ImportListResponse importListResponse)
    {
        var results = new List<ImportListItemInfo>();

        if (!PreProcess(importListResponse))
        {
            return results;
        }

        var resource = STJson.Deserialize<TResponse>(importListResponse.Content);
        results.AddRange(ParseResponse(resource));

        return results;
    }

    protected static ImportListItemInfo AsImportable(TmdbMediaResource resource)
    {
        // When MediaType is null/empty, it is implied to be a media type of "tv".
        if (resource.MediaType.IsNullOrWhiteSpace() || resource.MediaType == "tv")
        {
            return new ImportListItemInfo()
            {
                TmdbId = resource.Id,
                Title = resource.Name
            };
        }

        return null;
    }

    protected abstract IEnumerable<ImportListItemInfo> ParseResponse(TResponse resource);

    private static bool PreProcess(ImportListResponse importListResponse)
    {
        if (importListResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
        {
            throw new ImportListException(importListResponse,
                $"TMDb API(${importListResponse.Request.Url.Path}) call resulted in an unexpected StatusCode [{importListResponse.HttpResponse.StatusCode}]");
        }

        if (importListResponse.HttpResponse.Headers.ContentType != null && !importListResponse.HttpResponse.Headers.ContentType.Contains("text/json", StringComparison.OrdinalIgnoreCase) &&
            importListResponse.HttpRequest.Headers.Accept != null && importListResponse.HttpRequest.Headers.Accept.Contains("text/json", StringComparison.OrdinalIgnoreCase))
        {
            throw new ImportListException(importListResponse,
                $"TMDb API(${importListResponse.Request.Url.Path}) responded with html content. Site is likely blocked or unavailable.");
        }

        return true;
    }
}
