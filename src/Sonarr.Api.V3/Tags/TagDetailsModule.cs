using NzbDrone.Core.Tags;
using Sonarr.Http;

namespace Sonarr.Api.V3.Tags
{
    public class TagDetailsModule : SonarrRestModule<TagDetailsResource>
    {
        private readonly ITagService _tagService;

        public TagDetailsModule(ITagService tagService)
            : base("/tag/details")
        {
            _tagService = tagService;

            GetResourceById = Get;
        }

        private TagDetailsResource Get(int id)
        {
            return _tagService.Details(id).ToResource();
        }
    }
}
