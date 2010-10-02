using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Repository;
using TvdbLib.Data;

namespace NzbDrone.Core.Providers
{
    public interface ISeriesProvider
    {
        IQueryable<Series> GetSeries();
        Series GetSeries(long tvdbId);
        void SyncSeriesWithDisk();

        /// <summary>
        /// Parses a post title
        /// </summary>
        /// <param name="postTitle">Title of the report</param>
        /// <returns>TVDB id of the series this report belongs to</returns>
        long Parse(string postTitle);

        /// <summary>
        /// Determines if a series is being actively watched.
        /// </summary>
        /// <param name="id">The TVDB ID of the series</param>
        /// <returns>Whether or not the show is monitored</returns>
        bool IsMonitored(long id);

        bool RegisterSeries(string path);
        void RegisterSeries(string path, TvdbSeries series);
        List<String> GetUnmappedFolders();
    }
}