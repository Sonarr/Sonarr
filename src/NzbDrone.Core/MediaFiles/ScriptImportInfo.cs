using System.Collections.Generic;

namespace NzbDrone.Core.MediaFiles
{
    public struct ScriptImportInfo
    {
        public List<string> PossibleExtraFiles { get; set; }
        public string MediaFile { get; set; }

        public ScriptImportInfo(List<string> possibleExtraFiles, string mediaFile)
        {
            PossibleExtraFiles = possibleExtraFiles;
            MediaFile = mediaFile;
        }
    }
}
