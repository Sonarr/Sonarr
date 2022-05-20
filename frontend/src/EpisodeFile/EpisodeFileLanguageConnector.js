import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import EpisodeLanguage from 'Episode/EpisodeLanguage';
import createEpisodeFileSelector from 'Store/Selectors/createEpisodeFileSelector';

function createMapStateToProps() {
  return createSelector(
    createEpisodeFileSelector(),
    (episodeFile) => {
      return {
        language: episodeFile ? episodeFile.language : undefined
      };
    }
  );
}

export default connect(createMapStateToProps)(EpisodeLanguage);
