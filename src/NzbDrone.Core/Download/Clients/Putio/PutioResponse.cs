using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.Putio
{
    public class PutioGenericResponse
    {
        [JsonProperty(PropertyName = "error_message")]
        public string ErrorMessage { get; set; }

        public string Status { get; set; }
    }

    public class PutioTransfersResponse : PutioGenericResponse
    {
        public List<PutioTorrent> Transfers { get; set; }
    }

    public class PutioConfigResponse : PutioGenericResponse
    {
        public PutioTorrentMetadata Value { get; set; }
    }

    public class PutioAllConfigResponse : PutioGenericResponse
    {
        public Dictionary<string, PutioTorrentMetadata> Config { get; set; }
    }

    public class PutioFileListingResponse : PutioGenericResponse
    {
        public static PutioFileListingResponse Empty()
        {
            return new PutioFileListingResponse
            {
                Files = new List<PutioFile>(),
                Parent = new PutioFile
                {
                    Id = 0,
                    Name = "Your Files"
                }
            };
        }

        public List<PutioFile> Files { get; set; }

        public PutioFile Parent { get; set; }
    }
}
