using NzbDrone.Core.MediaFiles.MediaInfo;

namespace NzbDrone.Core.Extras.Metadata.Consumers.Xbmc
{
    public static class XbmcMetadataFormatter
    {
        public static string FormatAudioCodec(MediaInfoAudioStreamModel audioStream)
        {
            if (audioStream?.Format == null)
            {
                return string.Empty;
            }

            var audioFormat = audioStream.Format;
            var audioProfile = audioStream.Profile ?? string.Empty;

            // profile name definitions here https://github.com/FFmpeg/FFmpeg/blob/n8.1.2/libavcodec/profiles.c
            return audioFormat switch
            {
                // Missing Kodi dedicated codes for "DTS-ES" "DTS Express" "DTS 96/24"
                "dts" => audioProfile switch
                {
                    "DTS-HD HRA" => "dtshd_hra",
                    "DTS-HD MA" => "dtshd_ma",
                    "DTS-HD MA + DTS:X" => "dtshd_ma_x",
                    "DTS-HD MA + DTS:X IMAX" => "dtshd_ma_x_imax",
                    _ => audioFormat,
                },
                "truehd" => audioProfile switch
                {
                    "Dolby TrueHD + Dolby Atmos" => "truehd_atmos",
                    _ => audioFormat
                },
                "eac3" => audioProfile switch
                {
                    "Dolby Digital Plus + Dolby Atmos" => "eac3_ddp_atmos",
                    _ => audioFormat
                },

                // Missing Kodi dedicated codes for "LD", "ELD", "Main", "xHE-AAC"
                "aac" => audioProfile switch
                {
                    "LC" => "aac_lc",
                    "HE-AAC" => "he_aac",
                    "HE-AACv2" => "he_aac_v2",
                    "SSR" => "aac_ssr",
                    "LTP" => "aac_ltp",
                    _ => audioFormat,
                },
                _ => audioFormat
            };
        }
    }
}
