using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Datastore
{
    public class ModelConflictException : NzbDroneException
    {
        public ModelConflictException(Type modelType, int modelId)
            : base("{0} with ID {1} cannot be modified", modelType.Name, modelId)
        {
        }

        public ModelConflictException(Type modelType, int modelId, string message)
            : base("{0} with ID {1} {2}", modelType.Name, modelId, message)
        {
        }
    }
}
