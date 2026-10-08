using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.Processing;

namespace NzbDrone.Core.MediaCover
{
    public interface IImageResizer
    {
        void Resize(string source, string destination, int height);
    }

    public class ImageResizer : IImageResizer
    {
        private readonly IDiskProvider _diskProvider;
        private readonly DecoderOptions _decoderOptions;
        private readonly bool _enabled;

        public ImageResizer(IDiskProvider diskProvider, IPlatformInfo platformInfo)
        {
            _diskProvider = diskProvider;

            _enabled = true;

            var configuration = new SixLabors.ImageSharp.Configuration(new JpegConfigurationModule(), new PngConfigurationModule(), new WebpConfigurationModule())
            {
                // More conservative memory allocation
                MemoryAllocator = new SimpleGcMemoryAllocator()
            };

            // Thumbnails don't need super high quality
            configuration.ImageFormatsManager.SetEncoder(JpegFormat.Instance, new JpegEncoder
            {
                Quality = 92
            });

            _decoderOptions = new DecoderOptions
            {
                Configuration = configuration,
                SkipMetadata = true
            };
        }

        public void Resize(string source, string destination, int height)
        {
            if (!_enabled)
            {
                return;
            }

            try
            {
                using var image = Image.Load(_decoderOptions, source);
                image.Mutate(x => x.Resize(0, height));
                image.Save(destination);
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
