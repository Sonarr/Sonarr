namespace NzbDrone.Core.DataAugmentation.Xem.Model
{
    public class XemResult<T>
    {
        public string Result { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
    }
}
