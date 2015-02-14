using System;
using System.Collections.Generic;
using NzbDrone.Api.Mapping;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tags;
using NzbDrone.SignalR;

namespace NzbDrone.Api.Tags
{
    public class TagModule : NzbDroneRestModuleWithSignalR<TagResource, Tag>, IHandle<TagsUpdatedEvent>
    {
        private readonly ITagService _tagService;

        public TagModule(IBroadcastSignalRMessage signalRBroadcaster,
                         ITagService tagService)
            : base(signalRBroadcaster)
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

        public void Handle(TagsUpdatedEvent message)
        {
            BroadcastResourceChange(ModelAction.Sync);
        }
    }
}
