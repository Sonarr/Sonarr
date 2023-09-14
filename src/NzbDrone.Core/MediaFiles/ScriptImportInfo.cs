using System.Collections.Generic;

namespace NzbDrone.Core.MediaFiles
{
    public struct ScriptImportInfo
    {
        public List<string> PossibleExtraFiles { get; set; }
        public string MediaFile { get; set; }
        public ScriptImportDecision Decision { get; set; }
        public bool ImportExtraFiles { get; set; }

        public ScriptImportInfo(List<string> possibleExtraFiles, string mediaFile, ScriptImportDecision decision, bool importExtraFiles)
        {
            PossibleExtraFiles = possibleExtraFiles;
            MediaFile = mediaFile;
            Decision = decision;
            ImportExtraFiles = importExtraFiles;
        }
    }
}
