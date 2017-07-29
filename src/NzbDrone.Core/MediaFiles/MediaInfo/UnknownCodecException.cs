using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.MediaFiles.MediaInfo
{
    public class UnknownCodecException : Exception
    {
        public string Codec { get; set; }
        public string SceneName { get; set; }

        public UnknownCodecException(string codec, string sceneName)
            : base($"Unknown codec {codec}")
        {
            Codec = codec;
            SceneName = sceneName;
        }
    }
}
