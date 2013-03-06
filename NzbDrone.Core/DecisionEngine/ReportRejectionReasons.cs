namespace NzbDrone.Core.DecisionEngine
{
    public enum ReportRejectionReasons
    {
        None = 0,
        WrongSeries = 1,
        QualityNotWanted = 2,
        WrongSeason = 3,
        WrongEpisode = 4,
        Size = 5,
        Retention = 6,
        ExistingQualityIsEqualOrBetter = 7,
        Cutoff = 8,
        AlreadyInQueue = 9,
        DownloadClientFailure = 10,
        Skipped = 11,
        Failure = 12,
        ReleaseGroupNotWanted = 13,
        AiredAfterCustomStartDate = 14,
        LanguageNotWanted = 15
    }
}
