namespace NzbDrone.Core.Notifications.Discord
{
    public enum DiscordGrabFieldType
    {
        Overview,
        Rating,
        Genres,
        Quality,
        Group,
        Size,
        Links,
        Release,
        Poster,
        Fanart,
        CustomFormats,
        CustomFormatScore,
        Indexer
    }

    public enum DiscordImportFieldType
    {
        Overview,
        Rating,
        Genres,
        Quality,
        Codecs,
        Group,
        Size,
        Languages,
        Subtitles,
        Links,
        Release,
        Poster,
        Fanart
    }

    public enum DiscordManualInteractionFieldType
    {
        Overview,
        Rating,
        Genres,
        Quality,
        Group,
        Size,
        Links,
        DownloadTitle,
        Poster,
        Fanart
    }
}
