using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Api.Extensions;
using NzbDrone.Common;

namespace NzbDrone.Api.Directories
{
    public class DirectoryModule : NzbDroneApiModule
    {
        private readonly IDirectoryLookupService _directoryLookupService;

        public DirectoryModule(IDirectoryLookupService directoryLookupService)
            : base("/directories")
        {
            _directoryLookupService = directoryLookupService;
            Get["/"] = x => GetDirectories();
        }

        private Response GetDirectories()
        {
            if (!Request.Query.query.HasValue)
                return new List<string>().AsResponse();

            string query = Request.Query.query.Value;

            var dirs = _directoryLookupService.LookupSubDirectories(query)
                .Select(p => p.GetActualCasing())
                .ToList();

            return dirs.AsResponse();
        }
    }
}