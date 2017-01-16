namespace NzbDrone.Core.Download.Clients.NzbVortex
{
    public enum NzbVortexStateType
    {
        Waiting = 0,
        Downloading = 1,
        WaitingForSave = 2,
        Saving = 3,
        Saved = 4,
        PasswordRequest = 5,
        QuaedForProcessing = 6,
        UserWaitForProcessing = 7,
        Checking = 8,
        Repairing = 9,
        Joining = 10,
        WaitForFurtherProcessing = 11,
        Joining2 = 12,
        WaitForUncompress = 13,
        Uncompressing = 14,
        WaitForCleanup = 15,
        CleaningUp = 16,
        CleanedUp = 17,
        MovingToCompleted = 18,
        MoveCompleted = 19,
        Done = 20,
        UncompressFailed = 21,
        CheckFailedDataCorrupt = 22,
        MoveFailed = 23,
        BadlyEncoded = 24
    }
}
