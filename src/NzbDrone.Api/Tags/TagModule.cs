using System.Collections.Generic;
using NzbDrone.Core.Tags;

namespace NzbDrone.Api.Tags
{
    public class TagModule : NzbDroneRestModule<TagResource>
    {
        private readonly ITagService _tagService;

        public TagModule(ITagService tagService)
        {
            _tagService = tagService;

            GetResourceAll = GetAll;
        }

        private List<TagResource> GetAll()
        {
            return ToListResource(_tagService.All);
        }
    }
}
