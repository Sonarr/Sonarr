using System.Collections.Generic;
using TVDBSharp.Models;
using TVDBSharp.Models.DAO;
using TVDBSharp.Models.Enums;

namespace TVDBSharp
{
    /// <summary>
    ///     The main class which will handle all user interaction.
    /// </summary>
    public class TVDB
    {
        private readonly IDataProvider _dataProvider;

        /// <summary>
        ///     Creates a new instance with the provided API key and dataProvider.
        /// </summary>
        /// <param name="apiKey">The API key provided by TVDB.</param>
        /// <param name="dataProvider">Specify your own <see cref="IDataProvider" /> instance.</param>
        public TVDB(string apiKey, IDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
            _dataProvider.ApiKey = apiKey;
        }

        /// <summary>
        ///     Creates a new instance with the provided API key and standard <see cref="IDataProvider" />.
        /// </summary>
        /// <param name="apiKey">The API key provided by TVDB.</param>
        public TVDB(string apiKey)
        {
            _dataProvider = new DataProvider {ApiKey = apiKey};
        }

        /// <summary>
        ///     Search for a show in the database.
        /// </summary>
        /// <param name="query">Query that identifies the show.</param>
        /// <param name="results">Maximal amount of results in the returning set. Default is 5.</param>
        /// <returns>Returns a list of shows.</returns>
        public List<Show> Search(string query, int results = 5)
        {
            return new Builder(_dataProvider).Search(query, results);
        }

        /// <summary>
        ///     Get a specific show based on its ID.
        /// </summary>
        /// <param name="showId">ID of the show.</param>
        /// <returns>Returns the corresponding show.</returns>
        public Show GetShow(int showId)
        {
            return new Builder(_dataProvider).BuildShow(showId);
        }

        /// <summary>
        ///     Get a specific episode based on its ID.
        /// </summary>
        /// <param name="episodeId">ID of the episode</param>
        /// <param name="lang">ISO 639-1 language code for the episode</param>
        /// <returns>The corresponding episode</returns>
        public Episode GetEpisode(int episodeId, string lang = "en")
        {
            return new Builder(_dataProvider).BuildEpisode(episodeId, lang);
        }

        public Updates GetUpdates(Interval interval)
        {
            return new Builder(_dataProvider).BuildUpdates(interval);
        }
    }
}