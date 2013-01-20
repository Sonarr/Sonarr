using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Nancy;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Api.QualityType;

namespace NzbDrone.Api.QualityProfiles
{
    public class QualityProfileModule : NzbDroneApiModule
    {
        private readonly QualityProvider _qualityProvider;

        public QualityProfileModule(QualityProvider qualityProvider)
            : base("/QualityProfile")
        {
            _qualityProvider = qualityProvider;
            Get["/"] = x => OnGet();
            Get["/{Id}"] = x => OnGet((int)x.Id);
            Put["/"] = x => OnPut();
            Delete["/{Id}"] = x => OnDelete((int)x.Id);
        }

        private Response OnGet()
        {
            var profiles = _qualityProvider.All();
            return Mapper.Map<List<QualityProfile>, List<QualityProfileModel>>(profiles).AsResponse();
        }

        private Response OnGet(int id)
        {
            var profile = _qualityProvider.Get(id);
            return Mapper.Map<QualityProfile, QualityProfileModel>(profile).AsResponse();
        }

        private Response OnPost()
        {
            var request = Request.Body.FromJson<QualityProfileModel>();
            var profile = Mapper.Map<QualityProfileModel, QualityProfile>(request);
            request.Id = _qualityProvider.Add(profile);

            return request.AsResponse();
        }

        //Update
        private Response OnPut()
        {
            var request = Request.Body.FromJson<QualityProfileModel>();
            var profile = Mapper.Map<QualityProfileModel, QualityProfile>(request);
            _qualityProvider.Update(profile);

            return request.AsResponse();
        }

        private Response OnDelete(int id)
        {
            _qualityProvider.Delete(id);
            return new Response();
        }
    }
}