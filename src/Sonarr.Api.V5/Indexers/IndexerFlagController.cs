using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Parser.Model;
using Sonarr.Http;

namespace Sonarr.Api.V5.Indexers;

[V5ApiController]
public class IndexerFlagController : Controller
{
    [HttpGet]
    public List<IndexerFlagResource> GetAll()
    {
        return Enum.GetValues(typeof(IndexerFlags)).Cast<IndexerFlags>().Select(f => new IndexerFlagResource
        {
            Id = (int)f,
            Name = f.ToString()
        }).ToList();
    }
}
