using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using MongoDB.Bson;
using Nancy;
using NzbDrone.Services.Api.Extensions;

namespace NzbDrone.Services.Api.SceneMapping
{
    public class SceneMappingModule : NancyModule
    {
        private readonly SceneMappingRepository _sceneMappingRepository;

        public SceneMappingModule(SceneMappingRepository sceneMappingRepository)
            : base("/scenemapping")
        {
            _sceneMappingRepository = sceneMappingRepository;

            Get["/"] = x => OnGet();
            Get["/active"] = x => OnGet();
            Get["/{MapId}"] = x => OnGet(x.MapId);
        }

        private Response OnGet()
        {
            var mappings = _sceneMappingRepository.Public();
            return Mapper.Map<List<SceneMappingModel>, List<SceneMappingJsonModel>>(mappings).AsResponse();
        }

        private Response OnGet(string mapId)
        {
            var mapping = _sceneMappingRepository.Find(ObjectId.Parse(mapId));
            return Mapper.Map<SceneMappingModel, SceneMappingJsonModel>(mapping).AsResponse();
        }
    }
}