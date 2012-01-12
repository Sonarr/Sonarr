using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Datastore.Relators
{
    public class EpisodeFileEpisodeRelator
    {

        private EpisodeFile _current;
        public EpisodeFile MapIt(EpisodeFile episodeFile, Series series, Episode episode)
        {
            // Terminating call.  Since we can return null from this function
            // we need to be ready for PetaPoco to callback later with null
            // parameters
            if (episodeFile == null)
                return _current;

            // Is this the same EpisodeFile as the current one we're processing
            if (_current != null && _current.EpisodeFileId == episodeFile.EpisodeFileId)
            {
                // Yes, just add this post to the current EpisodeFiles's collection of Episodes
                _current.Episodes.Add(episode);

                // Return null to indicate we're not done with this EpisodeFiles yet
                return null;
            }

            // This is a different EpisodeFile to the current one, or this is the 
            // first time through and we don't have an EpisodeFile yet

            // Save the current EpisodeFile
            var prev = _current;

            // Setup the new current EpisodeFile
            _current = episodeFile;
            _current.Episodes = new List<Episode>();
            _current.Episodes.Add(episode);
            _current.Series = series;

            // Return the now populated previous EpisodeFile (or null if first time through)
            return prev;
        }

    }
}
