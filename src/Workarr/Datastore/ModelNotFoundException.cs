using Workarr.Exceptions;

namespace Workarr.Datastore
{
    public class ModelNotFoundException : WorkarrException
    {
        public ModelNotFoundException(Type modelType, int modelId)
            : base("{0} with ID {1} does not exist", modelType.Name, modelId)
        {
        }
    }
}
