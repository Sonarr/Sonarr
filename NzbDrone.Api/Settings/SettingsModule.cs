using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Api.Extensions;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Api.Settings
{
    public class SettingsModule : NzbDroneApiModule
    {
        private readonly IConfigService _configService;

        public SettingsModule(IConfigService configService)
            : base("/settings")
        {
            _configService = configService;
            Get["/"] = x => GetAllSettings();
            Post["/"] = x => SaveSettings();
        }

        private Response GetAllSettings()
        {
            var collection = Request.Query.Collection;

            if(collection.HasValue && Boolean.Parse(collection.Value))
                return _configService.All().AsResponse();

            return _configService.AllWithDefaults().AsResponse();
        }

        private Response SaveSettings()
        {
            var request = Request.Body.FromJson<Dictionary<string, object>>();

            return request.AsResponse();
        }
    }
}