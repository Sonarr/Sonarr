using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.MediaFiles.Events
{
    public enum ScanSkippedReason
    {
        RootFolderDoesNotExist,
        RootFolderIsEmpty,
        MediaFolderDoesNotExist
    }
}
