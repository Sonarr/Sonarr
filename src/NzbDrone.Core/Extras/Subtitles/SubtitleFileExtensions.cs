using System.Collections.Generic;

namespace NzbDrone.Core.Extras.Subtitles
{
    public static class SubtitleFileExtensions
    {
        private static HashSet<string> _fileExtensions;

        static SubtitleFileExtensions()
        {
            _fileExtensions = new HashSet<string>
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
                                  ".utf-8"
                              };
        }

        public static HashSet<string> Extensions => _fileExtensions;
    }
}
