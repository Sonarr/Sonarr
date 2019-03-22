namespace NzbDrone.Core.Update
{
    public enum UpdateMechanism
    {
        BuiltIn = 0,
        Script = 1,
        External = 10,
        Apt = 11,
        Docker = 12
    }
}
