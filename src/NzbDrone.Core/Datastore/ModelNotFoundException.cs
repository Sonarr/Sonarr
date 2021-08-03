using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Datastore
{
    public class ModelNotFoundException : NzbDroneException
    {
        public ModelNotFoundException(Type modelType, int modelId)
            : base("{0} with ID {1} does not exist", modelType.Name, modelId)
        {
        }
    }
}
