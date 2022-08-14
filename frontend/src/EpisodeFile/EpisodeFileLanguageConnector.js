import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import EpisodeLanguages from 'Episode/EpisodeLanguages';
import createEpisodeFileSelector from 'Store/Selectors/createEpisodeFileSelector';

function createMapStateToProps() {
  return createSelector(
    createEpisodeFileSelector(),
    (episodeFile) => {
      return {
        languages: episodeFile ? episodeFile.languages : undefined
      };
    }
  );
}

export default connect(createMapStateToProps)(EpisodeLanguages);
