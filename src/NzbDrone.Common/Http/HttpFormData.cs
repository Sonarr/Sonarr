namespace NzbDrone.Common.Http
{
    public class HttpFormData
    {
        public string Name { get; set; }
        public string FileName { get; set; }
        public byte[] ContentData { get; set; }
        public string ContentType { get; set; }
    }
}
