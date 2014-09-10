using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Tags
{
    public interface ITagService
    {
        Tag Add(Tag tag);
        void Delete(Int32 tagId);
        List<Tag> All();
    }

    public class TagService : ITagService
    {
        public Tag Add(Tag tag)
        {
            throw new NotImplementedException();
        }

        public void Delete(int tagId)
        {
            throw new NotImplementedException();
        }

        public List<Tag> All()
        {
            return new List<Tag>
                   {
                       new Tag { Id = 1, Label = "marktest" },
                       new Tag { Id = 2, Label = "mark-test" },
                       new Tag { Id = 3, Label = "mark_test" }
                   };
        }
    }
}
