using System;
using System.Collections.Generic;
using System.Net;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Notifications.Trakt.Resource;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Trakt
{
    public interface ITraktService
    {
        HttpRequest GetOAuthRequest(string callbackUrl);
        TraktAuthRefreshResource RefreshAuthToken(string refreshToken);
        void AddEpisodeToCollection(TraktSettings settings, Series series, EpisodeFile episodeFile);
        void RemoveEpisodeFromCollection(TraktSettings settings, Series series, EpisodeFile episodeFile);
        void RemoveSeriesFromCollection(TraktSettings settings, Series series);
        string GetUserName(string accessToken);
        ValidationFailure Test(TraktSettings settings);
    }

    public class TraktService : ITraktService
    {
        private readonly ITraktProxy _proxy;
        private readonly Logger _logger;

        public TraktService(ITraktProxy proxy,
                           Logger logger)
        {
            _proxy = proxy;
            _logger = logger;
        }

        public string GetUserName(string accessToken)
        {
            return _proxy.GetUserName(accessToken);
        }

        public HttpRequest GetOAuthRequest(string callbackUrl)
        {
            return _proxy.GetOAuthRequest(callbackUrl);
        }

        public TraktAuthRefreshResource RefreshAuthToken(string refreshToken)
        {
            return _proxy.RefreshAuthToken(refreshToken);
        }

        public ValidationFailure Test(TraktSettings settings)
        {
            try
            {
                GetUserName(settings.AccessToken);

                return null;
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.Error(ex, "Access Token is invalid: " + ex.Message);

                    return new ValidationFailure("Token", "Access Token is invalid");
                }

                _logger.Error(ex, "Unable to send test message: " + ex.Message);

                return new ValidationFailure("Token", "Unable to send test message");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test message: " + ex.Message);

                return new ValidationFailure("", "Unable to send test message");
            }
        }

        public void AddEpisodeToCollection(TraktSettings settings, Series series, EpisodeFile episodeFile)
        {
            var payload = new TraktCollectShowsResource
            {
                Shows = new List<TraktCollectShow>()
            };

            var traktResolution = MapResolution(episodeFile.Quality.Quality.Resolution, episodeFile.MediaInfo?.ScanType);
            var mediaType = MapMediaType(episodeFile.Quality.Quality.Source);
            var audio = MapAudio(episodeFile);
            var audioChannels = MapAudioChannels(episodeFile, audio);

            var payloadEpisodes = new List<TraktEpisodeResource>();

            foreach (var episode in episodeFile.Episodes.Value)
            {
                payloadEpisodes.Add(new TraktEpisodeResource
                {
                    Number = episode.EpisodeNumber,
                    CollectedAt = DateTime.Now,
                    Resolution = traktResolution,
                    MediaType = mediaType,
                    AudioChannels = audioChannels,
                    Audio = audio,
                });
            }

            var payloadSeasons = new List<TraktSeasonResource>();
            payloadSeasons.Add(new TraktSeasonResource
            {
                Number = episodeFile.SeasonNumber,
                Episodes = payloadEpisodes
            });

            payload.Shows.Add(new TraktCollectShow
            {
                Title = series.Title,
                Year = series.Year,
                Ids = new TraktShowIdsResource
                {
                    Tvdb = series.TvdbId,
                    Imdb = series.ImdbId ?? "",
                },
                Seasons = payloadSeasons,
            });

            _proxy.AddToCollection(payload, settings.AccessToken);
        }

        public void RemoveEpisodeFromCollection(TraktSettings settings, Series series, EpisodeFile episodeFile)
        {
            var payload = new TraktCollectShowsResource
            {
                Shows = new List<TraktCollectShow>()
            };

            var payloadEpisodes = new List<TraktEpisodeResource>();

            foreach (var episode in episodeFile.Episodes.Value)
            {
                payloadEpisodes.Add(new TraktEpisodeResource
                {
                    Number = episode.EpisodeNumber
                });
            }

            var payloadSeasons = new List<TraktSeasonResource>();
            payloadSeasons.Add(new TraktSeasonResource
            {
                Number = episodeFile.SeasonNumber,
                Episodes = payloadEpisodes
            });

            payload.Shows.Add(new TraktCollectShow
            {
                Title = series.Title,
                Year = series.Year,
                Ids = new TraktShowIdsResource
                {
                    Tvdb = series.TvdbId,
                    Imdb = series.ImdbId ?? "",
                },
                Seasons = payloadSeasons,
            });

            _proxy.RemoveFromCollection(payload, settings.AccessToken);
        }

        public void RemoveSeriesFromCollection(TraktSettings settings, Series series)
        {
            var payload = new TraktCollectShowsResource
            {
                Shows = new List<TraktCollectShow>()
            };

            payload.Shows.Add(new TraktCollectShow
            {
                Title = series.Title,
                Year = series.Year,
                Ids = new TraktShowIdsResource
                {
                    Tvdb = series.TvdbId,
                    Imdb = series.ImdbId ?? "",
                },
            });

            _proxy.RemoveFromCollection(payload, settings.AccessToken);
        }

        private string MapMediaType(QualitySource source)
        {
            var traktSource = string.Empty;

            switch (source)
            {
                case QualitySource.Web:
                case QualitySource.WebRip:
                    traktSource = "digital";
                    break;
                case QualitySource.BlurayRaw:
                case QualitySource.Bluray:
                    traktSource = "bluray";
                    break;
                case QualitySource.Television:
                case QualitySource.TelevisionRaw:
                    traktSource = "vhs";
                    break;
                case QualitySource.DVD:
                    traktSource = "dvd";
                    break;
            }

            return traktSource;
        }

        private string MapResolution(int resolution, string scanType)
        {
            var traktResolution = string.Empty;

            //var interlacedTypes = new string[] { "Interlaced", "MBAFF", "PAFF" };
            var scanIdentifier = scanType.IsNotNullOrWhiteSpace() && TraktInterlacedTypes.interlacedTypes.Contains(scanType) ? "i" : "p";

            switch (resolution)
            {
                case 2160:
                    traktResolution = "uhd_4k";
                    break;
                case 1080:
                    traktResolution = $"hd_1080{scanIdentifier}";
                    break;
                case 720:
                    traktResolution = "hd_720p";
                    break;
                case 576:
                    traktResolution = $"sd_576{scanIdentifier}";
                    break;
                case 480:
                    traktResolution = $"sd_480{scanIdentifier}";
                    break;
            }

            return traktResolution;
        }

        private string MapAudio(EpisodeFile episodeFile)
        {
            var traktAudioFormat = string.Empty;

            var audioCodec = episodeFile.MediaInfo != null ? MediaInfoFormatter.FormatAudioCodec(episodeFile.MediaInfo, episodeFile.SceneName) : string.Empty;

            switch (audioCodec)
            {
                case "AC3":
                    traktAudioFormat = "dolby_digital";
                    break;
                case "EAC3":
                    traktAudioFormat = "dolby_digital_plus";
                    break;
                case "TrueHD":
                    traktAudioFormat = "dolby_truehd";
                    break;
                case "EAC3 Atmos":
                    traktAudioFormat = "dolby_digital_plus_atmos";
                    break;
                case "TrueHD Atmos":
                    traktAudioFormat = "dolby_atmos";
                    break;
                case "DTS":
                case "DTS-ES":
                    traktAudioFormat = "dts";
                    break;
                case "DTS-HD MA":
                    traktAudioFormat = "dts_ma";
                    break;
                case "DTS-HD HRA":
                    traktAudioFormat = "dts_hr";
                    break;
                case "DTS-X":
                    traktAudioFormat = "dts_x";
                    break;
                case "MP3":
                    traktAudioFormat = "mp3";
                    break;
                case "MP2":
                    traktAudioFormat = "mp2";
                    break;
                case "Vorbis":
                    traktAudioFormat = "ogg";
                    break;
                case "WMA":
                    traktAudioFormat = "wma";
                    break;
                case "AAC":
                    traktAudioFormat = "aac";
                    break;
                case "PCM":
                    traktAudioFormat = "lpcm";
                    break;
                case "FLAC":
                    traktAudioFormat = "flac";
                    break;
                case "Opus":
                    traktAudioFormat = "ogg_opus";
                    break;
            }

            return traktAudioFormat;
        }

        private string MapAudioChannels(EpisodeFile episodeFile, string audioFormat)
        {
            var audioChannels = episodeFile.MediaInfo != null ? MediaInfoFormatter.FormatAudioChannels(episodeFile.MediaInfo).ToString("0.0") : string.Empty;

            return audioChannels;
        }
    }
}
