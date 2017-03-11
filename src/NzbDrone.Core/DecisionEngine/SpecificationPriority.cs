namespace NzbDrone.Core.DecisionEngine
{
    public enum SpecificationPriority
    {
        Default = 0,
        Parsing = 0,
        Database = 0,
        Disk = 1
    }
}
