using System;
using System.Collections.Generic;
using Nancy;
using NzbDrone.Api.Extensions;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Api.Config
{
    public class SettingsModule : NzbDroneApiModule
    {
        private readonly IConfigService _configService;
        private readonly IConfigFileProvider _configFileProvider;

        public SettingsModule(IConfigService configService, IConfigFileProvider configFileProvider)
            : base("/settings")
        {
            _configService = configService;
            _configFileProvider = configFileProvider;
            Get["/"] = x => GetGeneralSettings();
            Post["/"] = x => SaveGeneralSettings();

            Get["/host"] = x => GetHostSettings();          
            Post["/host"] = x => SaveHostSettings();

            Get["/log"] = x => GetLogSettings();
            Post["/log"] = x => SaveLogSettings();
        }

        private Response SaveLogSettings()
        {
            throw new NotImplementedException();
        }

        private Response GetLogSettings()
        {
            throw new NotImplementedException();
        }

        private Response SaveHostSettings()
        {
            var request = Request.Body.FromJson<Dictionary<string, object>>();
            _configFileProvider.SaveConfigDictionary(request);

            return GetHostSettings();
        }

        private Response GetHostSettings()
        {
            return _configFileProvider.GetConfigDictionary().AsResponse();
        }

        private Response GetGeneralSettings()
        {
            var collection = Request.Query.Collection;

            if (collection.HasValue && Boolean.Parse(collection.Value))
                return _configService.All().AsResponse();

            return _configService.AllWithDefaults().AsResponse();
        }

        private Response SaveGeneralSettings()
        {
            var request = Request.Body.FromJson<Dictionary<string, object>>();
            _configService.SaveValues(request);


            return request.AsResponse();
        }
    }
}