using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using TvdbLib.Data;

namespace NzbDrone.Core.Providers
{
    public interface ISeriesProvider
    {
        IQueryable<Series> GetAllSeries();
        Series GetSeries(int seriesId);

        /// <summary>
        /// Determines if a series is being actively watched.
        /// </summary>
        /// <param name="id">The TVDB ID of the series</param>
        /// <returns>Whether or not the show is monitored</returns>
        bool IsMonitored(long id);

        TvdbSeries MapPathToSeries(string path);
        void AddSeries(string path, TvdbSeries series);
        Dictionary<Guid, String> GetUnmappedFolders();
        Series FindSeries(string cleanTitle);
        bool QualityWanted(int seriesId, QualityTypes quality);
        void UpdateSeries(Series series);
    }
}