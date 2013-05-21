using System;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Datastore.Events
{
    public class ModelEvent<T> : IEvent where T : ModelBase
    {
        public T Model { get; set; }
        public RepositoryAction Action { get; set; }

        public ModelEvent(T model, RepositoryAction action)
        {
            Model = model;
            Action = action;
        }
    }

    public enum RepositoryAction
    {
        Created = 1,
        Updated = 2,
        Deleted = 3
    }


}