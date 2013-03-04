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
        private readonly ConfigService _configService;

        public SettingsModule(ConfigService configService)
            : base("/settings")
        {
            _configService = configService;
            Get["/"] = x => GetAllSettings();
        }

        private Response GetAllSettings()
        {
            var collection = Request.Query.Collection;

            if(collection.HasValue && Boolean.Parse(collection.Value))
                return _configService.All().AsResponse();

            return _configService.AllWithDefaults().AsResponse();
        }
    }
}