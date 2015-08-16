using System.Collections.Generic;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.MetadataSource
{
    public interface ISearchForNewMovie
    {
        IEnumerable<Movie> SearchForNewMovie(string title);
    }
}
