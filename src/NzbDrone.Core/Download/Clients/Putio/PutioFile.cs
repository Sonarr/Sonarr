using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.Putio
{
    public class PutioFile
    {
        public static string FILE_TYPE_FOLDER = "FOLDER";
        public static string FILE_TYPE_VIDEO = "VIDEO";
        public long Id { get; set; }
        public string Name { get; set; }
        [JsonProperty(PropertyName = "parent_id")]
        public long ParentId { get; set; }
        [JsonProperty(PropertyName = "file_type")]
        public string FileType { get; set; }
        public bool IsFolder()
        {
            return FileType == FILE_TYPE_FOLDER;
        }
    }
}
