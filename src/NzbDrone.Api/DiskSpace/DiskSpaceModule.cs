using System.Collections.Generic;
using NzbDrone.Core.DiskSpace;
using Sonarr.Http;

namespace NzbDrone.Api.DiskSpace
{
    public class DiskSpaceModule :SonarrRestModule<DiskSpaceResource>
    {
        private readonly IDiskSpaceService _diskSpaceService;

        public DiskSpaceModule(IDiskSpaceService diskSpaceService)
            : base("diskspace")
        {
            _diskSpaceService = diskSpaceService;
            GetResourceAll = GetFreeSpace;
        }


        public List<DiskSpaceResource> GetFreeSpace()
        {
            return _diskSpaceService.GetFreeSpace().ConvertAll(DiskSpaceResourceMapper.MapToResource);
        }
    }
}
