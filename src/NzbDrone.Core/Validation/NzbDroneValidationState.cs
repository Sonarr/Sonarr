namespace NzbDrone.Core.Validation
{
    public class NzbDroneValidationState
    {
        public static NzbDroneValidationState Warning = new NzbDroneValidationState { IsWarning = true };

        public bool IsWarning { get; set; }
    }
}
