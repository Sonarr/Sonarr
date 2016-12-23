using ImageResizer;
using NzbDrone.Common.Disk;

namespace NzbDrone.Core.MediaCover
{
    public interface IImageResizer
    {
        void Resize(string source, string destination, int height);
    }

    public class ImageResizer : IImageResizer
    {
        private readonly IDiskProvider _diskProvider;

        public ImageResizer(IDiskProvider diskProvider)
        {
            _diskProvider = diskProvider;
        }

        public void Resize(string source, string destination, int height)
        {
            try
            {
                GdiPlusInterop.CheckGdiPlus();

                using (var sourceStream = _diskProvider.OpenReadStream(source))
                {
                    using (var outputStream = _diskProvider.OpenWriteStream(destination))
                    {
                        var settings = new Instructions();
                        settings.Height = height;

                        var job = new ImageJob(sourceStream, outputStream, settings);

                        ImageBuilder.Current.Build(job);
                    }
                }
            }
            catch
            {
                if (_diskProvider.FileExists(destination))
                {
                    _diskProvider.DeleteFile(destination);
                }
                throw;
            }
        }
    }
}
