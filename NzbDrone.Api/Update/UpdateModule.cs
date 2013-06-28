using System;
using System.Collections.Generic;
using NzbDrone.Api.REST;
using NzbDrone.Core.Update;
using NzbDrone.Api.Mapping;

namespace NzbDrone.Api.Update
{
    public class UpdateModule : NzbDroneRestModule<UpdateResource>
    {
        private readonly ICheckUpdateService _checkUpdateService;

        public UpdateModule(ICheckUpdateService checkUpdateService)
        {
            _checkUpdateService = checkUpdateService;
            GetResourceAll = GetAvailableUpdate;
        }

        private List<UpdateResource> GetAvailableUpdate()
        {
            var update = _checkUpdateService.AvailableUpdate();
            var response = new List<UpdateResource>();

            if (update != null)
            {
                response.Add(update.InjectTo<UpdateResource>());
            }

            return response;
        }
    }

    public class UpdateResource : RestResource
    {
        public Version Version { get; set; }
        public String FileName { get; set; }
        public String Url { get; set; }
    }
}