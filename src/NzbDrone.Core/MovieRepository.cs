using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Movies
{
    public interface IMovieRepository : IBasicRepository<Movie>
    {
        Movie FindByTitle(string cleanTitle);
        Movie FindByImdbId(string ImdbId);
        Movie FindByTmdbId(string TmbdId);
        bool MoviePathExists(string path);

    }

    public class MovieRepository : BasicRepository<Movie>,IMovieRepository
    {
        public MovieRepository(IDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        { }

        public Movie FindByTitle(string cleanTitle)
        {
           return Query.SingleOrDefault(s=>s.CleanTitle.Equals(cleanTitle,StringComparison.InvariantCultureIgnoreCase));
        }

        public Movie FindByImdbId(string ImdbId)
        {
            return Query.SingleOrDefault(s=>s.ImdbId.Equals(ImdbId));
        }

        public Movie FindByTmdbId(string TmbdId)
        {
            return Query.SingleOrDefault(s=>s.TmdbId.Equals(TmbdId));
        }

        public bool MoviePathExists(string path)
        {
            return Query.Any(c => c.Path == path);
        }
    }
}
