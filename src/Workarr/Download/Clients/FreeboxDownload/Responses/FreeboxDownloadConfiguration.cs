using Newtonsoft.Json;

namespace Workarr.Download.Clients.FreeboxDownload.Responses
{
    public class FreeboxDownloadConfiguration
    {
        [JsonProperty(PropertyName = "download_dir")]
        public string DownloadDirectory { get; set; }
        public string DecodedDownloadDirectory
        {
            get
            {
                return DownloadDirectory.DecodeBase64();
            }
            set
            {
                DownloadDirectory = value.EncodeBase64();
            }
        }
    }
}
