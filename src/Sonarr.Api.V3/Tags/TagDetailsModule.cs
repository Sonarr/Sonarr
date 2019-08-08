using System.Collections.Generic;
using NzbDrone.Core.Tags;
using Sonarr.Http;

namespace Sonarr.Api.V3.Tags
{
    public class TagDetailsModule : SonarrRestModule<TagDetailsResource>
    {
        private readonly ITagService _tagService;

        public TagDetailsModule(ITagService tagService)
            : base("/tag/detail")
        {
            _tagService = tagService;

            GetResourceById = GetTagDetails;
            GetResourceAll = GetAll;
        }

        private TagDetailsResource GetTagDetails(int id)
        {
            return _tagService.Details(id).ToResource();
        }

        private List<TagDetailsResource> GetAll()
        {
            return _tagService.Details().ToResource();
        }
    }
}
