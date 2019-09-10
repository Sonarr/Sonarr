using NzbDrone.Common.Disk;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using SixLabors.Memory;

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

            // More conservative memory allocation
            SixLabors.ImageSharp.Configuration.Default.MemoryAllocator = new SimpleGcMemoryAllocator();

            // Thumbnails don't need super high quality
            SixLabors.ImageSharp.Configuration.Default.ImageFormatsManager.SetEncoder(JpegFormat.Instance, new JpegEncoder
            {
                Quality = 92
            });
        }

        public void Resize(string source, string destination, int height)
        {
            try
            {
                using (var image = Image.Load(source))
                {
                    image.Mutate(x => x.Resize(0, height));
                    image.Save(destination);
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
