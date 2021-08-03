using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Profiles.Releases;
using Sonarr.Http;

namespace Sonarr.Api.V3.Profiles.Release
{
    public class ReleaseProfileModule : SonarrRestModule<ReleaseProfileResource>
    {
        private readonly IReleaseProfileService _releaseProfileService;
        private readonly IIndexerFactory _indexerFactory;

        public ReleaseProfileModule(IReleaseProfileService releaseProfileService, IIndexerFactory indexerFactory)
        {
            _releaseProfileService = releaseProfileService;
            _indexerFactory = indexerFactory;

            GetResourceById = GetReleaseProfile;
            GetResourceAll = GetAll;
            CreateResource = Create;
            UpdateResource = Update;
            DeleteResource = DeleteReleaseProfile;

            SharedValidator.RuleFor(d => d).Custom((restriction, context) =>
            {
                if (restriction.MapIgnored().Empty() && restriction.MapRequired().Empty() && restriction.Preferred.Empty())
                {
                    context.AddFailure("'Must contain', 'Must not contain' or 'Preferred' is required");
                }

                if (restriction.Enabled && restriction.IndexerId != 0 && !_indexerFactory.Exists(restriction.IndexerId))
                {
                    context.AddFailure(nameof(ReleaseProfile.IndexerId), "Indexer does not exist");
                }

                if (restriction.Preferred.Any(p => p.Key.IsNullOrWhiteSpace()))
                {
                    context.AddFailure("Preferred", "Term cannot be empty or consist of only spaces");
                }
            });
        }

        private ReleaseProfileResource GetReleaseProfile(int id)
        {
            return _releaseProfileService.Get(id).ToResource();
        }

        private List<ReleaseProfileResource> GetAll()
        {
            return _releaseProfileService.All().ToResource();
        }

        private int Create(ReleaseProfileResource resource)
        {
            return _releaseProfileService.Add(resource.ToModel()).Id;
        }

        private void Update(ReleaseProfileResource resource)
        {
            _releaseProfileService.Update(resource.ToModel());
        }

        private void DeleteReleaseProfile(int id)
        {
            _releaseProfileService.Delete(id);
        }
    }
}
