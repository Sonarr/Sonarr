using System.Linq;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.MediaFiles.MediaInfo
{
    public static class MediaInfoModelExtensions
    {
        private const string HlgTransferFunction = "HLG";
        private const string PgTransferFunction = "PQ";
        private const string ValidHdrColourPrimaries = "BT.2020";

        private static readonly string[] Hdr10Formats = { "SMPTE ST 2086", "HDR10" };
        private static readonly string[] Hdr10PlusFormats = { "SMPTE ST 2094 App 4" };
        private static readonly string[] DolbyVisionFormats = { "Dolby Vision" };

        public static HdrFormat GetHdrFormat(this MediaInfoModel mediaInfo)
        {
            if (mediaInfo.VideoBitDepth < 10)
            {
                return HdrFormat.None;
            }

            if (mediaInfo.VideoHdrFormat.IsNotNullOrWhiteSpace())
            {
                if (DolbyVisionFormats.Any(mediaInfo.VideoHdrFormat.ContainsIgnoreCase))
                {
                    if (Hdr10Formats.Any(mediaInfo.VideoHdrFormat.ContainsIgnoreCase))
                    {
                        return HdrFormat.DolbyVisionHdr10;
                    }
                    else
                    {
                        return HdrFormat.DolbyVision;
                    }
                }
                else if (Hdr10Formats.Any(mediaInfo.VideoHdrFormat.ContainsIgnoreCase))
                {
                    return HdrFormat.Hdr10;
                }
                else if (Hdr10PlusFormats.Any(mediaInfo.VideoHdrFormat.ContainsIgnoreCase))
                {
                    return HdrFormat.Hdr10Plus;
                }
            }

            // We didn't match straight from the format from MediaInfo, so try and match in ColourPrimaries and TransferCharacteristics
            if (mediaInfo.VideoColourPrimaries.IsNotNullOrWhiteSpace() && mediaInfo.VideoTransferCharacteristics.IsNotNullOrWhiteSpace())
            {
                if (mediaInfo.VideoColourPrimaries.EqualsIgnoreCase(ValidHdrColourPrimaries))
                {
                    if (mediaInfo.VideoTransferCharacteristics.EqualsIgnoreCase(PgTransferFunction))
                    {
                        return HdrFormat.Pq10;
                    }
                    else if (mediaInfo.VideoTransferCharacteristics.EqualsIgnoreCase(HlgTransferFunction))
                    {
                        return HdrFormat.Hlg10;
                    }
                }
            }

            if (mediaInfo.VideoHdrFormat.IsNotNullOrWhiteSpace())
            {
                return HdrFormat.UnknownHdr; // MediaInfo reported Hdr information, but we're unsure of what type.
            }

            return HdrFormat.None;
        }
    }
}
