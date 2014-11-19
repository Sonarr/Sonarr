using System;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Tags
{
    public interface ITagService
    {
        Tag GetTag(Int32 tagId);
        List<Tag> All();
        Tag Add(Tag tag);
        Tag Update(Tag tag);
        void Delete(Int32 tagId);
    }

    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;

        public TagService(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        public Tag GetTag(Int32 tagId)
        {
            return _tagRepository.Get(tagId);
        }

        public List<Tag> All()
        {
            return _tagRepository.All().ToList();
        }

        public Tag Add(Tag tag)
        {
            return _tagRepository.Insert(tag);
        }

        public Tag Update(Tag tag)
        {
            return _tagRepository.Update(tag);
        }

        public void Delete(Int32 tagId)
        {
            _tagRepository.Delete(tagId);
        }
    }
}
