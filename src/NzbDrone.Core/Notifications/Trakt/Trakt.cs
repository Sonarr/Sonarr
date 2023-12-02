using System;
using System.Collections.Generic;
using System.Net;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Localization;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Notifications.Trakt.Resource;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Trakt
{
    public class Trakt : NotificationBase<TraktSettings>
    {
        private readonly ITraktProxy _proxy;
        private readonly INotificationRepository _notificationRepository;
        private readonly ILocalizationService _localizationService;
        private readonly Logger _logger;

        public Trakt(ITraktProxy proxy, INotificationRepository notificationRepository, ILocalizationService localizationService, Logger logger)
        {
            _proxy = proxy;
            _notificationRepository = notificationRepository;
            _localizationService = localizationService;
            _logger = logger;
        }

        public override string Link => "https://trakt.tv/";
        public override string Name => "Trakt";

        public override void OnDownload(DownloadMessage message)
        {
            RefreshTokenIfNecessary();
            AddEpisodeToCollection(Settings, message.Series, message.EpisodeFile);
        }

        public override void OnEpisodeFileDelete(EpisodeDeleteMessage deleteMessage)
        {
            RefreshTokenIfNecessary();
            RemoveEpisodeFromCollection(Settings, deleteMessage.Series, deleteMessage.EpisodeFile);
        }

        public override void OnSeriesAdd(SeriesAddMessage message)
        {
            RefreshTokenIfNecessary();
            AddSeriesToCollection(Settings, message.Series);
        }

        public override void OnSeriesDelete(SeriesDeleteMessage deleteMessage)
        {
            RefreshTokenIfNecessary();
            RemoveSeriesFromCollection(Settings, deleteMessage.Series);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            RefreshTokenIfNecessary();

            try
            {
                _proxy.GetUserName(Settings.AccessToken);
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.Error(ex, "Access Token is invalid: " + ex.Message);

                    failures.Add(new ValidationFailure("Token", _localizationService.GetLocalizedString("NotificationsValidationInvalidAccessToken")));
                }
                else
                {
                    _logger.Error(ex, "Unable to send test message: " + ex.Message);

                    failures.Add(new ValidationFailure("Token", _localizationService.GetLocalizedString("NotificationsValidationUnableToSendTestMessage", new Dictionary<string, object> { { "exceptionMessage", ex.Message } })));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test message: " + ex.Message);

                failures.Add(new ValidationFailure("", _localizationService.GetLocalizedString("NotificationsValidationUnableToSendTestMessage", new Dictionary<string, object> { { "exceptionMessage", ex.Message } })));
            }

            return new ValidationResult(failures);
        }

        public override object RequestAction(string action, IDictionary<string, string> query)
        {
            if (action == "startOAuth")
            {
                var request = _proxy.GetOAuthRequest(query["callbackUrl"]);

                return new
                {
                    OauthUrl = request.Url.ToString()
                };
            }
            else if (action == "getOAuthToken")
            {
                return new
                {
                    accessToken = query["access_token"],
                    expires = DateTime.UtcNow.AddSeconds(int.Parse(query["expires_in"])),
                    refreshToken = query["refresh_token"],
                    authUser = _proxy.GetUserName(query["access_token"])
                };
            }

            return new { };
        }

        private void RefreshTokenIfNecessary()
        {
            if (Settings.Expires < DateTime.UtcNow.AddMinutes(5))
            {
                RefreshToken();
            }
        }

        private void RefreshToken()
        {
            _logger.Trace("Refreshing Token");

            Settings.Validate().Filter("RefreshToken").ThrowOnError();

            try
            {
                var response = _proxy.RefreshAuthToken(Settings.RefreshToken);

                if (response != null)
                {
                    var token = response;

                    Settings.AccessToken = token.AccessToken;
                    Settings.Expires = DateTime.UtcNow.AddSeconds(token.ExpiresIn);
                    Settings.RefreshToken = token.RefreshToken ?? Settings.RefreshToken;

                    if (Definition.Id > 0)
                    {
                        _notificationRepository.UpdateSettings((NotificationDefinition)Definition);
                    }
                }
            }
            catch (HttpException ex)
            {
                _logger.Warn(ex, "Error refreshing trakt access token");
            }
        }

        private void AddEpisodeToCollection(TraktSettings settings, Series series, EpisodeFile episodeFile)
        {
            var payload = new TraktCollectShowsResource
            {
                Shows = new List<TraktCollectShow>()
            };

            var traktResolution = MapResolution(episodeFile.Quality.Quality.Resolution, episodeFile.MediaInfo?.ScanType);
            var hdr = MapHdr(episodeFile);
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
                    Hdr = hdr,
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

        private void RemoveEpisodeFromCollection(TraktSettings settings, Series series, EpisodeFile episodeFile)
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

        private void AddSeriesToCollection(TraktSettings settings, Series series)
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
                }
            });

            _proxy.AddToCollection(payload, settings.AccessToken);
        }

        private void RemoveSeriesFromCollection(TraktSettings settings, Series series)
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
            var traktSource = source switch
            {
                QualitySource.Web => "digital",
                QualitySource.WebRip => "digital",
                QualitySource.BlurayRaw => "bluray",
                QualitySource.Bluray => "bluray",
                QualitySource.Television => "vhs",
                QualitySource.TelevisionRaw => "vhs",
                QualitySource.DVD => "dvd",
                _ => string.Empty
            };

            return traktSource;
        }

        private string MapResolution(int resolution, string scanType)
        {
            var scanIdentifier = scanType.IsNotNullOrWhiteSpace() && TraktInterlacedTypes.InterlacedTypes.Contains(scanType) ? "i" : "p";

            var traktResolution = resolution switch
            {
                2160 => "uhd_4k",
                1080 => $"hd_1080{scanIdentifier}",
                720 => "hd_720p",
                576 => $"sd_576{scanIdentifier}",
                480 => $"sd_480{scanIdentifier}",
                _ => string.Empty
            };

            return traktResolution;
        }

        private string MapHdr(EpisodeFile episodeFile)
        {
            var traktHdr = episodeFile.MediaInfo?.VideoHdrFormat switch
            {
                HdrFormat.DolbyVision or HdrFormat.DolbyVisionSdr => "dolby_vision",
                HdrFormat.Hdr10 or HdrFormat.DolbyVisionHdr10 => "hdr10",
                HdrFormat.Hdr10Plus or HdrFormat.DolbyVisionHdr10Plus => "hdr10_plus",
                HdrFormat.Hlg10 or HdrFormat.DolbyVisionHlg => "hlg",
                _ => null
            };

            return traktHdr;
        }

        private string MapAudio(EpisodeFile episodeFile)
        {
            var audioCodec = episodeFile.MediaInfo != null ? MediaInfoFormatter.FormatAudioCodec(episodeFile.MediaInfo, episodeFile.SceneName) : string.Empty;

            var traktAudioFormat = audioCodec switch
            {
                "AC3" => "dolby_digital",
                "EAC3" => "dolby_digital_plus",
                "TrueHD" => "dolby_truehd",
                "EAC3 Atmos" => "dolby_digital_plus_atmos",
                "TrueHD Atmos" => "dolby_atmos",
                "DTS" => "dts",
                "DTS-ES" => "dts",
                "DTS-HD MA" => "dts_ma",
                "DTS-HD HRA" => "dts_hr",
                "DTS-X" => "dts_x",
                "MP3" => "mp3",
                "MP2" => "mp2",
                "Vorbis" => "ogg",
                "WMA" => "wma",
                "AAC" => "aac",
                "PCM" => "lpcm",
                "FLAC" => "flac",
                "Opus" => "ogg_opus",
                _ => string.Empty
            };

            return traktAudioFormat;
        }

        private string MapAudioChannels(EpisodeFile episodeFile, string audioFormat)
        {
            var audioChannels = episodeFile.MediaInfo != null ? MediaInfoFormatter.FormatAudioChannels(episodeFile.MediaInfo).ToString("0.0") : string.Empty;

            return audioChannels;
        }
    }
}
