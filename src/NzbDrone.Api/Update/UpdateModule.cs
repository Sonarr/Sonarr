using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NzbDrone.Api.REST;
using NzbDrone.Core.Update;
using NzbDrone.Api.Mapping;

namespace NzbDrone.Api.Update
{
    public class UpdateModule : NzbDroneRestModule<UpdateResource>
    {
        private readonly ICheckUpdateService _checkUpdateService;
        private readonly IRecentUpdateProvider _recentUpdateProvider;

        public UpdateModule(ICheckUpdateService checkUpdateService,
                            IRecentUpdateProvider recentUpdateProvider)
        {
            _checkUpdateService = checkUpdateService;
            _recentUpdateProvider = recentUpdateProvider;
            GetResourceAll = GetRecentUpdates;
        }

        private UpdateResource GetAvailableUpdate()
        {
            var update = _checkUpdateService.AvailableUpdate();
            var response = new UpdateResource();

            if (update != null)
            {
                return update.InjectTo<UpdateResource>();
            }

            return response;
        }

        private List<UpdateResource> GetRecentUpdates()
        {
            return ToListResource(_recentUpdateProvider.GetRecentUpdatePackages);
        }
    }

    public class UpdateResource : RestResource
    {
        [JsonConverter(typeof(Newtonsoft.Json.Converters.VersionConverter))]
        public Version Version { get; set; }

        public String Branch { get; set; }
        public DateTime ReleaseDate { get; set; }
        public String FileName { get; set; }
        public String Url { get; set; }

        public UpdateChanges Changes { get; set; }
    }
}