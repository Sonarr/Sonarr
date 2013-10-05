using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR.Hosting;
using Nancy;
using Nancy.ModelBinding;
using Newtonsoft.Json;
using NzbDrone.Api.Extensions;
using NzbDrone.Api.REST;
using NzbDrone.Core.Update;
using NzbDrone.Api.Mapping;

namespace NzbDrone.Api.Update
{
    public class UpdateModule : NzbDroneRestModule<UpdateResource>
    {
        private readonly IRecentUpdateProvider _recentUpdateProvider;
        private readonly IInstallUpdates _installUpdateService;

        public UpdateModule(IRecentUpdateProvider recentUpdateProvider,
                            IInstallUpdates installUpdateService)
        {
            _recentUpdateProvider = recentUpdateProvider;
            _installUpdateService = installUpdateService;
            GetResourceAll = GetRecentUpdates;
            Post["/"] = x=> InstallUpdate();
        }

        private List<UpdateResource> GetRecentUpdates()
        {
            var resources = _recentUpdateProvider.GetRecentUpdatePackages()
                                                 .OrderByDescending(u => u.Version)
                                                 .InjectTo<List<UpdateResource>>();

            if (resources.Any())
            {
                resources.First().Latest = true;
            }

            return resources;
        }

        private Response InstallUpdate()
        {
            var updateResource = Request.Body.FromJson<UpdateResource>();

            var updatePackage = updateResource.InjectTo<UpdatePackage>();
            _installUpdateService.InstallUpdate(updatePackage);

            return updateResource.AsResponse();
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
        public Boolean Latest { get; set; }
        public UpdateChanges Changes { get; set; }
    }
}