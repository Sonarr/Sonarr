namespace NzbDrone.Core.MediaFiles
{
    public enum ScriptImportDecision
    {
        MoveComplete,
        RenameRequested,
        RejectExtra,
        DeferMove
    }
}
