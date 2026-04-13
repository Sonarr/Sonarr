using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.DiskSpace;
using Sonarr.Http;

namespace Sonarr.Api.V5.DiskSpace;

[V5ApiController("diskspace")]
public class DiskSpaceController : Controller
{
    private readonly IDiskSpaceService _diskSpaceService;

    public DiskSpaceController(IDiskSpaceService diskSpaceService)
    {
        _diskSpaceService = diskSpaceService;
    }

    [HttpGet]
    [Produces("application/json")]
    public Ok<List<DiskSpaceResource>> GetFreeSpace()
    {
        return TypedResults.Ok(_diskSpaceService.GetFreeSpace().ConvertAll(DiskSpaceResourceMapper.MapToResource));
    }
}
