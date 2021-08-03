using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Datastore.Events
{
    public class ModelEvent<TModel> : IEvent
        where TModel : ModelBase
    {
        public int ModelId { get; set; }
        public TModel Model { get; set; }
        public ModelAction Action { get; set; }

        public ModelEvent(int modelId, ModelAction action)
        {
            ModelId = modelId;
            Action = action;
        }

        public ModelEvent(TModel model, ModelAction action)
        {
            ModelId = model.Id;
            Model = model;
            Action = action;
        }
    }

    public enum ModelAction
    {
        Unknown = 0,
        Created = 1,
        Updated = 2,
        Deleted = 3,
        Sync = 4
    }
}
