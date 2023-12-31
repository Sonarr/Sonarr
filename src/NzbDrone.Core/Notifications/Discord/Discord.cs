using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Localization;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Notifications.Discord.Payloads;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Discord
{
    public class Discord : NotificationBase<DiscordSettings>
    {
        private readonly IDiscordProxy _proxy;
        private readonly ILocalizationService _localizationService;

        public Discord(IDiscordProxy proxy, ILocalizationService localizationService)
        {
            _proxy = proxy;
            _localizationService = localizationService;
        }

        public override string Name => "Discord";
        public override string Link => "https://support.discordapp.com/hc/en-us/articles/228383668-Intro-to-Webhooks";

        public override void OnGrab(GrabMessage message)
        {
            var series = message.Series;
            var episodes = message.Episode.Episodes;

            var embed = new Embed
            {
                Author = new DiscordAuthor
                {
                    Name = Settings.Author.IsNullOrWhiteSpace() ? Environment.MachineName : Settings.Author,
                    IconUrl = "https://raw.githubusercontent.com/Sonarr/Sonarr/develop/Logo/256.png"
                },
                Url = $"http://thetvdb.com/?tab=series&id={series.TvdbId}",
                Description = "Episode Grabbed",
                Title = GetTitle(series, episodes),
                Color = (int)DiscordColors.Standard,
                Fields = new List<DiscordField>(),
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            };

            if (Settings.GrabFields.Contains((int)DiscordGrabFieldType.Poster))
            {
                embed.Thumbnail = new DiscordImage
                {
                    Url = series.Images.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Poster)?.RemoteUrl
                };
            }

            if (Settings.GrabFields.Contains((int)DiscordGrabFieldType.Fanart))
            {
                embed.Image = new DiscordImage
                {
                    Url = series.Images.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Fanart)?.RemoteUrl
                };
            }

            foreach (var field in Settings.GrabFields)
            {
                var discordField = new DiscordField();

                switch ((DiscordGrabFieldType)field)
                {
                    case DiscordGrabFieldType.Overview:
                        var overview = episodes.First().Overview ?? "";
                        discordField.Name = "Overview";
                        discordField.Value = overview.Length <= 300 ? overview : $"{overview.AsSpan(0, 300)}...";
                        break;
                    case DiscordGrabFieldType.Rating:
                        discordField.Name = "Rating";
                        discordField.Value = episodes.First().Ratings.Value.ToString();
                        break;
                    case DiscordGrabFieldType.Genres:
                        discordField.Name = "Genres";
                        discordField.Value = series.Genres.Take(5).Join(", ");
                        break;
                    case DiscordGrabFieldType.Quality:
                        discordField.Name = "Quality";
                        discordField.Inline = true;
                        discordField.Value = message.Quality.Quality.Name;
                        break;
                    case DiscordGrabFieldType.Group:
                        discordField.Name = "Group";
                        discordField.Value = message.Episode.ParsedEpisodeInfo.ReleaseGroup;
                        break;
                    case DiscordGrabFieldType.Size:
                        discordField.Name = "Size";
                        discordField.Value = BytesToString(message.Episode.Release.Size);
                        discordField.Inline = true;
                        break;
                    case DiscordGrabFieldType.Release:
                        discordField.Name = "Release";
                        discordField.Value = string.Format("```{0}```", message.Episode.Release.Title);
                        break;
                    case DiscordGrabFieldType.Links:
                        discordField.Name = "Links";
                        discordField.Value = GetLinksString(series);
                        break;
                    case DiscordGrabFieldType.CustomFormats:
                        discordField.Name = "Custom Formats";
                        discordField.Value = string.Join("|", message.Episode.CustomFormats);
                        break;
                    case DiscordGrabFieldType.CustomFormatScore:
                        discordField.Name = "Custom Format Score";
                        discordField.Value = message.Episode.CustomFormatScore.ToString();
                        break;
                    case DiscordGrabFieldType.Indexer:
                        discordField.Name = "Indexer";
                        discordField.Value = message.Episode.Release.Indexer;
                        break;
                }

                if (discordField.Name.IsNotNullOrWhiteSpace() && discordField.Value.IsNotNullOrWhiteSpace())
                {
                    embed.Fields.Add(discordField);
                }
            }

            var payload = CreatePayload(null, new List<Embed> { embed });

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnDownload(DownloadMessage message)
        {
            var series = message.Series;
            var episodes = message.EpisodeFile.Episodes.Value;
            var isUpgrade = message.OldFiles.Count > 0;

            var embed = new Embed
            {
                Author = new DiscordAuthor
                {
                    Name = Settings.Author.IsNullOrWhiteSpace() ? Environment.MachineName : Settings.Author,
                    IconUrl = "https://raw.githubusercontent.com/Sonarr/Sonarr/develop/Logo/256.png"
                },
                Url = $"http://thetvdb.com/?tab=series&id={series.TvdbId}",
                Description = isUpgrade ? "Episode Upgraded" : "Episode Imported",
                Title = GetTitle(series, episodes),
                Color = isUpgrade ? (int)DiscordColors.Upgrade : (int)DiscordColors.Success,
                Fields = new List<DiscordField>(),
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            };

            if (Settings.ImportFields.Contains((int)DiscordImportFieldType.Poster))
            {
                embed.Thumbnail = new DiscordImage
                {
                    Url = series.Images.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Poster)?.RemoteUrl
                };
            }

            if (Settings.ImportFields.Contains((int)DiscordImportFieldType.Fanart))
            {
                embed.Image = new DiscordImage
                {
                    Url = series.Images.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Fanart)?.RemoteUrl
                };
            }

            foreach (var field in Settings.ImportFields)
            {
                var discordField = new DiscordField();

                switch ((DiscordImportFieldType)field)
                {
                    case DiscordImportFieldType.Overview:
                        var overview = episodes.First().Overview ?? "";
                        discordField.Name = "Overview";
                        discordField.Value = overview.Length <= 300 ? overview : $"{overview.AsSpan(0, 300)}...";
                        break;
                    case DiscordImportFieldType.Rating:
                        discordField.Name = "Rating";
                        discordField.Value = episodes.First().Ratings.Value.ToString();
                        break;
                    case DiscordImportFieldType.Genres:
                        discordField.Name = "Genres";
                        discordField.Value = series.Genres.Take(5).Join(", ");
                        break;
                    case DiscordImportFieldType.Quality:
                        discordField.Name = "Quality";
                        discordField.Inline = true;
                        discordField.Value = message.EpisodeFile.Quality.Quality.Name;
                        break;
                    case DiscordImportFieldType.Codecs:
                        discordField.Name = "Codecs";
                        discordField.Inline = true;
                        discordField.Value = string.Format("{0} / {1} {2}",
                            MediaInfoFormatter.FormatVideoCodec(message.EpisodeFile.MediaInfo, null),
                            MediaInfoFormatter.FormatAudioCodec(message.EpisodeFile.MediaInfo, null),
                            MediaInfoFormatter.FormatAudioChannels(message.EpisodeFile.MediaInfo));
                        break;
                    case DiscordImportFieldType.Group:
                        discordField.Name = "Group";
                        discordField.Value = message.EpisodeFile.ReleaseGroup;
                        break;
                    case DiscordImportFieldType.Size:
                        discordField.Name = "Size";
                        discordField.Value = BytesToString(message.EpisodeFile.Size);
                        discordField.Inline = true;
                        break;
                    case DiscordImportFieldType.Languages:
                        discordField.Name = "Languages";
                        discordField.Value = message.EpisodeFile.MediaInfo.AudioLanguages.ConcatToString("/");
                        break;
                    case DiscordImportFieldType.Subtitles:
                        discordField.Name = "Subtitles";
                        discordField.Value = message.EpisodeFile.MediaInfo.Subtitles.ConcatToString("/");
                        break;
                    case DiscordImportFieldType.Release:
                        discordField.Name = "Release";
                        discordField.Value = string.Format("```{0}```", message.EpisodeFile.SceneName);
                        break;
                    case DiscordImportFieldType.Links:
                        discordField.Name = "Links";
                        discordField.Value = GetLinksString(series);
                        break;
                }

                if (discordField.Name.IsNotNullOrWhiteSpace() && discordField.Value.IsNotNullOrWhiteSpace())
                {
                    embed.Fields.Add(discordField);
                }
            }

            var payload = CreatePayload(null, new List<Embed> { embed });

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnRename(Series series, List<RenamedEpisodeFile> renamedFiles)
        {
            var attachments = new List<Embed>
                              {
                                  new Embed
                                  {
                                      Title = series.Title,
                                  }
                              };

            var payload = CreatePayload("Renamed", attachments);

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnEpisodeFileDelete(EpisodeDeleteMessage deleteMessage)
        {
            var series = deleteMessage.Series;
            var episodes = deleteMessage.EpisodeFile.Episodes;
            var deletedFile = deleteMessage.EpisodeFile.Path;
            var reason = deleteMessage.Reason;

            var embed = new Embed
            {
                Author = new DiscordAuthor
                {
                    Name = Settings.Author.IsNullOrWhiteSpace() ? Environment.MachineName : Settings.Author,
                    IconUrl = "https://raw.githubusercontent.com/Sonarr/Sonarr/develop/Logo/256.png"
                },
                Url = $"http://thetvdb.com/?tab=series&id={series.TvdbId}",
                Title = GetTitle(series, episodes),
                Description = "Episode Deleted",
                Color = (int)DiscordColors.Danger,
                Fields = new List<DiscordField>
                {
                    new DiscordField { Name = "Reason", Value = reason.ToString() },
                    new DiscordField { Name = "File name", Value = string.Format("```{0}```", deletedFile) }
                },
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            };

            var payload = CreatePayload(null, new List<Embed> { embed });

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnSeriesAdd(SeriesAddMessage message)
        {
            var series = message.Series;
            var embed = new Embed
            {
                Author = new DiscordAuthor
                {
                    Name = Settings.Author.IsNullOrWhiteSpace() ? Environment.MachineName : Settings.Author,
                    IconUrl = "https://raw.githubusercontent.com/Sonarr/Sonarr/develop/Logo/256.png"
                },
                Url = $"http://thetvdb.com/?tab=series&id={series.TvdbId}",
                Title = series.Title,
                Description = "Series Added",
                Color = (int)DiscordColors.Success,
                Fields = new List<DiscordField> { new DiscordField { Name = "Links", Value = GetLinksString(series) } }
            };

            if (Settings.ImportFields.Contains((int)DiscordImportFieldType.Poster))
            {
                embed.Thumbnail = new DiscordImage
                {
                    Url = series.Images.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Poster)?.RemoteUrl
                };
            }

            if (Settings.ImportFields.Contains((int)DiscordImportFieldType.Fanart))
            {
                embed.Image = new DiscordImage
                {
                    Url = series.Images.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Fanart)?.RemoteUrl
                };
            }

            var payload = CreatePayload(null, new List<Embed> { embed });

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnSeriesDelete(SeriesDeleteMessage deleteMessage)
        {
            var series = deleteMessage.Series;

            var embed = new Embed
            {
                Author = new DiscordAuthor
                {
                    Name = Settings.Author.IsNullOrWhiteSpace() ? Environment.MachineName : Settings.Author,
                    IconUrl = "https://raw.githubusercontent.com/Sonarr/Sonarr/develop/Logo/256.png"
                },
                Url = $"http://thetvdb.com/?tab=series&id={series.TvdbId}",
                Title = series.Title,
                Description = deleteMessage.DeletedFilesMessage,
                Color = (int)DiscordColors.Danger,
                Fields = new List<DiscordField> { new DiscordField { Name = "Links", Value = GetLinksString(series) } }
            };

            if (Settings.ImportFields.Contains((int)DiscordImportFieldType.Poster))
            {
                embed.Thumbnail = new DiscordImage
                {
                    Url = series.Images.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Poster)?.RemoteUrl
                };
            }

            if (Settings.ImportFields.Contains((int)DiscordImportFieldType.Fanart))
            {
                embed.Image = new DiscordImage
                {
                    Url = series.Images.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Fanart)?.RemoteUrl
                };
            }

            var payload = CreatePayload(null, new List<Embed> { embed });

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {
            var embed = new Embed
            {
                Author = new DiscordAuthor
                {
                    Name = Settings.Author.IsNullOrWhiteSpace() ? Environment.MachineName : Settings.Author,
                    IconUrl = "https://raw.githubusercontent.com/Sonarr/Sonarr/develop/Logo/256.png"
                },
                Title = healthCheck.Source.Name,
                Description = healthCheck.Message,
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                Color = healthCheck.Type == HealthCheck.HealthCheckResult.Warning ? (int)DiscordColors.Warning : (int)DiscordColors.Danger
            };

            var payload = CreatePayload(null, new List<Embed> { embed });

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnHealthRestored(HealthCheck.HealthCheck previousCheck)
        {
            var embed = new Embed
            {
                Author = new DiscordAuthor
                {
                    Name = Settings.Author.IsNullOrWhiteSpace() ? Environment.MachineName : Settings.Author,
                    IconUrl = "https://raw.githubusercontent.com/Sonarr/Sonarr/develop/Logo/256.png"
                },
                Title = "Health Issue Resolved: " + previousCheck.Source.Name,
                Description = $"The following issue is now resolved: {previousCheck.Message}",
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                Color = (int)DiscordColors.Success
            };

            var payload = CreatePayload(null, new List<Embed> { embed });

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnApplicationUpdate(ApplicationUpdateMessage updateMessage)
        {
            var embed = new Embed
            {
                Author = new DiscordAuthor
                {
                    Name = Settings.Author.IsNullOrWhiteSpace() ? Environment.MachineName : Settings.Author,
                    IconUrl = "https://raw.githubusercontent.com/Sonarr/Sonarr/develop/Logo/256.png"
                },
                Title = APPLICATION_UPDATE_TITLE,
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                Color = (int)DiscordColors.Standard,
                Fields = new List<DiscordField>()
                {
                    new DiscordField()
                    {
                        Name = "Previous Version",
                        Value = updateMessage.PreviousVersion.ToString()
                    },
                    new DiscordField()
                    {
                        Name = "New Version",
                        Value = updateMessage.NewVersion.ToString()
                    }
                },
            };

            var payload = CreatePayload(null, new List<Embed> { embed });

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnManualInteractionRequired(ManualInteractionRequiredMessage message)
        {
            var series = message.Series;
            var episodes = message.Episode.Episodes;

            var embed = new Embed
            {
                Author = new DiscordAuthor
                {
                    Name = Settings.Author.IsNullOrWhiteSpace() ? Environment.MachineName : Settings.Author,
                    IconUrl = "https://raw.githubusercontent.com/Sonarr/Sonarr/develop/Logo/256.png"
                },
                Url = $"http://thetvdb.com/?tab=series&id={series.TvdbId}",
                Description = "Manual interaction needed",
                Title = GetTitle(series, episodes),
                Color = (int)DiscordColors.Standard,
                Fields = new List<DiscordField>(),
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            };

            if (Settings.ManualInteractionFields.Contains((int)DiscordManualInteractionFieldType.Poster))
            {
                embed.Thumbnail = new DiscordImage
                {
                    Url = series.Images.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Poster)?.Url
                };
            }

            if (Settings.ManualInteractionFields.Contains((int)DiscordManualInteractionFieldType.Fanart))
            {
                embed.Image = new DiscordImage
                {
                    Url = series.Images.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Fanart)?.Url
                };
            }

            foreach (var field in Settings.ManualInteractionFields)
            {
                var discordField = new DiscordField();

                switch ((DiscordManualInteractionFieldType)field)
                {
                    case DiscordManualInteractionFieldType.Overview:
                        var overview = episodes.First().Overview ?? "";
                        discordField.Name = "Overview";
                        discordField.Value = overview.Length <= 300 ? overview : $"{overview.AsSpan(0, 300)}...";
                        break;
                    case DiscordManualInteractionFieldType.Rating:
                        discordField.Name = "Rating";
                        discordField.Value = episodes.First().Ratings.Value.ToString();
                        break;
                    case DiscordManualInteractionFieldType.Genres:
                        discordField.Name = "Genres";
                        discordField.Value = series.Genres.Take(5).Join(", ");
                        break;
                    case DiscordManualInteractionFieldType.Quality:
                        discordField.Name = "Quality";
                        discordField.Inline = true;
                        discordField.Value = message.Quality.Quality.Name;
                        break;
                    case DiscordManualInteractionFieldType.Group:
                        discordField.Name = "Group";
                        discordField.Value = message.Episode.ParsedEpisodeInfo.ReleaseGroup;
                        break;
                    case DiscordManualInteractionFieldType.Size:
                        discordField.Name = "Size";
                        discordField.Value = BytesToString(message.TrackedDownload.DownloadItem.TotalSize);
                        discordField.Inline = true;
                        break;
                    case DiscordManualInteractionFieldType.DownloadTitle:
                        discordField.Name = "Download";
                        discordField.Value = string.Format("```{0}```", message.TrackedDownload.DownloadItem.Title);
                        break;
                    case DiscordManualInteractionFieldType.Links:
                        discordField.Name = "Links";
                        discordField.Value = GetLinksString(series);
                        break;
                }

                if (discordField.Name.IsNotNullOrWhiteSpace() && discordField.Value.IsNotNullOrWhiteSpace())
                {
                    embed.Fields.Add(discordField);
                }
            }

            var payload = CreatePayload(null, new List<Embed> { embed });

            _proxy.SendPayload(payload, Settings);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(TestMessage());

            return new ValidationResult(failures);
        }

        public ValidationFailure TestMessage()
        {
            try
            {
                var message = $"Test message from Sonarr posted at {DateTime.Now}";
                var payload = CreatePayload(message);

                _proxy.SendPayload(payload, Settings);
            }
            catch (DiscordException ex)
            {
                return new NzbDroneValidationFailure(string.Empty, _localizationService.GetLocalizedString("NotificationsValidationUnableToSendTestMessage", new Dictionary<string, object> { { "exceptionMessage", ex.Message } }));
            }

            return null;
        }

        private DiscordPayload CreatePayload(string message, List<Embed> embeds = null)
        {
            var avatar = Settings.Avatar;

            var payload = new DiscordPayload
            {
                Username = Settings.Username,
                Content = message,
                Embeds = embeds
            };

            if (avatar.IsNotNullOrWhiteSpace())
            {
                payload.AvatarUrl = avatar;
            }

            if (Settings.Username.IsNotNullOrWhiteSpace())
            {
                payload.Username = Settings.Username;
            }

            return payload;
        }

        private string BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; // Longs run out around EB
            if (byteCount == 0)
            {
                return "0 " + suf[0];
            }

            var bytes = Math.Abs(byteCount);
            var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            var num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return string.Format("{0} {1}", (Math.Sign(byteCount) * num).ToString(), suf[place]);
        }

        private string GetLinksString(Series series)
        {
            var links = new List<string>();

            links.Add($"[The TVDB](https://thetvdb.com/?tab=series&id={series.TvdbId})");
            links.Add($"[Trakt](https://trakt.tv/search/tvdb/{series.TvdbId}?id_type=show)");

            if (series.ImdbId.IsNotNullOrWhiteSpace())
            {
                links.Add($"[IMDB](https://imdb.com/title/{series.ImdbId}/)");
            }

            return string.Join(" / ", links);
        }

        private string GetTitle(Series series, List<Episode> episodes)
        {
            if (series.SeriesType == SeriesTypes.Daily)
            {
                var episode = episodes.First();

                return $"{series.Title} - {episode.AirDate} - {episode.Title}";
            }

            var episodeNumbers = string.Concat(episodes.Select(e => e.EpisodeNumber)
                                                       .Select(i => string.Format("x{0:00}", i)));

            var episodeTitles = string.Join(" + ", episodes.Select(e => e.Title));

            var title = $"{series.Title} - {episodes.First().SeasonNumber}{episodeNumbers} - {episodeTitles}";

            return title.Length > 256 ? $"{title.AsSpan(0, 253)}..." : title;
        }
    }
}
