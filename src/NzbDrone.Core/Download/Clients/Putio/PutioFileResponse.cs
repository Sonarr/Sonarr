namespace NzbDrone.Core.Download.Clients.Putio
{
    public class PutioFileResponse : PutioGenericResponse
    {
        public PutioFile File { get; set; }
    }
}
