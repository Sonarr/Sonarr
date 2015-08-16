﻿using System;
using System.Collections.Generic;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Movies;
using NzbDrone.Core.MediaFiles.Series;
using NzbDrone.Core.MediaFiles.Movies;
using NzbDrone.Core.MediaFiles.MediaInfo;

namespace NzbDrone.Core.Organizer
{
    public interface IFilenameSampleService
    {
        SampleResult GetStandardSample(NamingConfig nameSpec);
        SampleResult GetMultiEpisodeSample(NamingConfig nameSpec);
        SampleResult GetDailySample(NamingConfig nameSpec);
        SampleResult GetAnimeSample(NamingConfig nameSpec);
        SampleResult GetAnimeMultiEpisodeSample(NamingConfig nameSpec);
        SampleResult GetStandardMovieSample(NamingConfig nameSpec);
        String GetSeriesFolderSample(NamingConfig nameSpec);
        String GetSeasonFolderSample(NamingConfig nameSpec);
        String GetMovieFolderSample(NamingConfig nameSpec);
    }

    public class FileNameSampleService : IFilenameSampleService
    {
        private readonly IBuildFileNames _buildFileNames;
        private static Series _standardSeries;
        private static Series _dailySeries;
        private static Series _animeSeries;
        private static Episode _episode1;
        private static Episode _episode2;
        private static Episode _episode3;
        private static List<Episode> _singleEpisode;
        private static List<Episode> _multiEpisodes;
        private static EpisodeFile _singleEpisodeFile;
        private static EpisodeFile _multiEpisodeFile;
        private static EpisodeFile _dailyEpisodeFile;
        private static EpisodeFile _animeEpisodeFile;
        private static EpisodeFile _animeMultiEpisodeFile;
        private static Movie _standardMovie;
        private static MovieFile _standardMovieFile;

        public FileNameSampleService(IBuildFileNames buildFileNames)
        {
            _buildFileNames = buildFileNames;

            _standardMovie = new Movie
            {
                Title = "Movie Title",
                Year = 2010,
                OriginalTitle = "Movie Original Title"
            };


            _standardSeries = new Series
            {
                SeriesType = SeriesTypes.Standard,
                Title = "Series Title (2010)"
            };

            _dailySeries = new Series
            {
                SeriesType = SeriesTypes.Daily,
                Title = "Series Title (2010)"
            };

            _animeSeries = new Series
            {
                SeriesType = SeriesTypes.Anime,
                Title = "Series Title (2010)"
            };

            _episode1 = new Episode
            {
                SeasonNumber = 1,
                EpisodeNumber = 1,
                Title = "Episode Title (1)",
                AirDate = "2013-10-30",
                AbsoluteEpisodeNumber = 1,
            };

            _episode2 = new Episode
            {
                SeasonNumber = 1,
                EpisodeNumber = 2,
                Title = "Episode Title (2)",
                AbsoluteEpisodeNumber = 2
            };

            _episode3 = new Episode
            {
                SeasonNumber = 1,
                EpisodeNumber = 3,
                Title = "Episode Title (3)",
                AbsoluteEpisodeNumber = 3
            };

            _singleEpisode = new List<Episode> { _episode1 };
            _multiEpisodes = new List<Episode> { _episode1, _episode2, _episode3 };

            var mediaInfo = new MediaInfoModel()
            {
                VideoCodec = "AVC",
                AudioFormat = "DTS",
                AudioLanguages = "English",
                Subtitles = "English/German"
            };

            var mediaInfoAnime = new MediaInfoModel()
            {
                VideoCodec = "AVC",
                AudioFormat = "DTS",
                AudioLanguages = "Japanese",
                Subtitles = "Japanese/English"
            };

            _singleEpisodeFile = new EpisodeFile
            {
                Quality = new QualityModel(Quality.HDTV720p, new Revision(2)),
                RelativePath = "Series.Title.S01E01.720p.HDTV.x264-EVOLVE.mkv",
                SceneName = "Series.Title.S01E01.720p.HDTV.x264-EVOLVE",
                ReleaseGroup = "RlsGrp",
                MediaInfo = mediaInfo
            };

            _multiEpisodeFile = new EpisodeFile
            {
                Quality = new QualityModel(Quality.HDTV720p, new Revision(2)),
                RelativePath = "Series.Title.S01E01-E03.720p.HDTV.x264-EVOLVE.mkv",
                SceneName = "Series.Title.S01E01-E03.720p.HDTV.x264-EVOLVE",
                ReleaseGroup = "RlsGrp",
                MediaInfo = mediaInfo,
            };

            _dailyEpisodeFile = new EpisodeFile
            {
                Quality = new QualityModel(Quality.HDTV720p, new Revision(2)),
                RelativePath = "Series.Title.2013.10.30.HDTV.x264-EVOLVE.mkv",
                SceneName = "Series.Title.2013.10.30.HDTV.x264-EVOLVE",
                ReleaseGroup = "RlsGrp",
                MediaInfo = mediaInfo
            };

            _animeEpisodeFile = new EpisodeFile
            {
                Quality = new QualityModel(Quality.HDTV720p, new Revision(2)),
                RelativePath = "[RlsGroup] Series Title - 001 [720p].mkv",
                SceneName = "[RlsGroup] Series Title - 001 [720p]",
                ReleaseGroup = "RlsGrp",
                MediaInfo = mediaInfoAnime
            };

            _animeMultiEpisodeFile = new EpisodeFile
            {
                Quality = new QualityModel(Quality.HDTV720p, new Revision(2)),
                RelativePath = "[RlsGroup] Series Title - 001 - 103 [720p].mkv",
                SceneName = "[RlsGroup] Series Title - 001 - 103 [720p]",
                ReleaseGroup = "RlsGrp",
                MediaInfo = mediaInfoAnime
            };

            _standardMovieFile = new MovieFile
            {
                Quality = new QualityModel(Quality.HDTV720p, new Revision(2)),
                RelativePath = "Movie.Title.2010.720p.HDTV.x264-EVOLVE.mkv",
                SceneName = "Movie.Title.2010.720p.HDTV.x264-EVOLVE",
                ReleaseGroup = "RlsGrp",
                MediaInfo = mediaInfo
            };


        }

        public SampleResult GetStandardSample(NamingConfig nameSpec)
        {
            var result = new SampleResult
            {
                FileName = BuildSample(_singleEpisode, _standardSeries, _singleEpisodeFile, nameSpec),
                Series = _standardSeries,
                Episodes = _singleEpisode,
                EpisodeFile = _singleEpisodeFile
            };

            return result;
        }

        public SampleResult GetStandardMovieSample(NamingConfig nameSpec)
        {
            var result = new SampleResult
            {
                FileName = BuildSample(_standardMovie, _standardMovieFile, nameSpec),
                Movie = _standardMovie,
                MovieFile = _standardMovieFile
            };

            return result;
        }

        public SampleResult GetMultiEpisodeSample(NamingConfig nameSpec)
        {
            var result = new SampleResult
            {
                FileName = BuildSample(_multiEpisodes, _standardSeries, _multiEpisodeFile, nameSpec),
                Series = _standardSeries,
                Episodes = _multiEpisodes,
                EpisodeFile = _multiEpisodeFile
            };

            return result;
        }

        public SampleResult GetDailySample(NamingConfig nameSpec)
        {
            var result = new SampleResult
            {
                FileName = BuildSample(_singleEpisode, _dailySeries, _dailyEpisodeFile, nameSpec),
                Series = _dailySeries,
                Episodes = _singleEpisode,
                EpisodeFile = _dailyEpisodeFile
            };

            return result;
        }

        public SampleResult GetAnimeSample(NamingConfig nameSpec)
        {
            var result = new SampleResult
            {
                FileName = BuildSample(_singleEpisode, _animeSeries, _animeEpisodeFile, nameSpec),
                Series = _animeSeries,
                Episodes = _singleEpisode,
                EpisodeFile = _animeEpisodeFile
            };

            return result;
        }

        public SampleResult GetAnimeMultiEpisodeSample(NamingConfig nameSpec)
        {
            var result = new SampleResult
            {
                FileName = BuildSample(_multiEpisodes, _animeSeries, _animeMultiEpisodeFile, nameSpec),
                Series = _animeSeries,
                Episodes = _multiEpisodes,
                EpisodeFile = _animeMultiEpisodeFile
            };

            return result;
        }

        public string GetSeriesFolderSample(NamingConfig nameSpec)
        {
            return _buildFileNames.GetSeriesFolder(_standardSeries, nameSpec);
        }

        public string GetMovieFolderSample(NamingConfig nameSpec)
        {
            return _buildFileNames.GetMovieFolder(_standardMovie, nameSpec);
        }

        public string GetSeasonFolderSample(NamingConfig nameSpec)
        {
            return _buildFileNames.GetSeasonFolder(_standardSeries, _episode1.SeasonNumber, nameSpec);
        }

        private string BuildSample(List<Episode> episodes, Series series, EpisodeFile episodeFile, NamingConfig nameSpec)
        {
            try
            {
                return _buildFileNames.BuildFileName(episodes, series, episodeFile, nameSpec);
            }
            catch (NamingFormatException)
            {
                return String.Empty;
            }
        }

        private string BuildSample(Movie movie, MovieFile movieFile, NamingConfig nameSpec)
        {
            try
            {
                return _buildFileNames.BuildFileName(movie, movieFile, nameSpec);
            }
            catch (NamingFormatException)
            {
                return String.Empty;
            }
        }

    }
}
