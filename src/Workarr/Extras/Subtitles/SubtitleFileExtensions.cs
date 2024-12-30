namespace Workarr.Extras.Subtitles
{
    public static class SubtitleFileExtensions
    {
        private static HashSet<string> _fileExtensions;

        static SubtitleFileExtensions()
        {
            _fileExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                              {
                                  ".aqt",
                                  ".ass",
                                  ".idx",
                                  ".jss",
                                  ".psb",
                                  ".rt",
                                  ".smi",
                                  ".srt",
                                  ".ssa",
                                  ".sub",
                                  ".txt",
                                  ".utf",
                                  ".utf8",
                                  ".utf-8",
                                  ".vtt"
                              };
        }

        public static HashSet<string> Extensions => _fileExtensions;
    }
}
