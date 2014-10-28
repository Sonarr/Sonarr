using System;
using System.Collections.Generic;
using NzbDrone.Api.Mapping;
using NzbDrone.Core.Tags;

namespace NzbDrone.Api.Tags
{
    public class TagModule : NzbDroneRestModule<TagResource>
    {
        private readonly ITagService _tagService;

        public TagModule(ITagService tagService)
        {
            _tagService = tagService;

            GetResourceById = Get;
            GetResourceAll = GetAll;
            CreateResource = Create;
            UpdateResource = Update;
            DeleteResource = Delete;
        }

        private TagResource Get(Int32 id)
        {
            return _tagService.GetTag(id).InjectTo<TagResource>();
        }

        private List<TagResource> GetAll()
        {
            return ToListResource(_tagService.All);
        }

        private Int32 Create(TagResource resource)
        {
            return _tagService.Add(resource.InjectTo<Tag>()).Id;
        }

        private void Update(TagResource resource)
        {
            _tagService.Update(resource.InjectTo<Tag>());
        }

        private void Delete(Int32 id)
        {
            _tagService.Delete(id);
        }
    }
}
