using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Entities;
using TvdbLib.Data;

namespace NzbDrone.Core.Providers
{
    public interface ISeriesProvider
    {
        IQueryable<Series> GetSeries();
        Series GetSeries(int seriesId);
        void SyncSeriesWithDisk();

        /// <summary>
        /// Determines if a series is being actively watched.
        /// </summary>
        /// <param name="id">The TVDB ID of the series</param>
        /// <returns>Whether or not the show is monitored</returns>
        bool IsMonitored(long id);

        TvdbSeries MapPathToSeries(string path);
        void RegisterSeries(string path, TvdbSeries series);
        List<String> GetUnmappedFolders();
    }
}