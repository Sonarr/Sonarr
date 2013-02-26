using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Repository;

// ReSharper disable CheckNamespace
namespace PetaPoco
{
    public class EpisodeSeasonRelator
    {
        public Season _current;
        public Season MapIt(Season season, Episode episode, EpisodeFile episodeFile)
        {
            // Terminating call. Since we can return null from this function
            // we need to be ready for PetaPoco to callback later with null
            // parameters
            if (season == null)
                return _current;

            //Todo: Find a Query that doesn't require this check
            //Map EpisodeFile to Episode (Map to null if 0, because PetaPoco is returning a POCO when it should be null)
            episode.EpisodeFile = (episode.EpisodeFileId == 0 ?  null : episodeFile);

            // Is this the same season as the current one we're processing
            if (_current != null && _current.Id == season.Id)
            {
                // Yes, just add this post to the current author's collection of posts
                _current.Episodes.Add(episode);

                // Return null to indicate we're not done with this author yet
                return null;
            }

            // This is season different author to the current one, or this is the 
            // first time through and we don't have an season yet

            // Save the current author
            var prev = _current;

            // Setup the new current season
            _current = season;
            _current.Episodes = new List<Episode>();
            _current.Episodes.Add(episode);

            // Return the now populated previous season (or null if first time through)
            return prev;
        }

    }
}
