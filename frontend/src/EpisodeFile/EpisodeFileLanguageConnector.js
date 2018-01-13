import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createEpisodeFileSelector from 'Store/Selectors/createEpisodeFileSelector';
import EpisodeLanguage from 'Episode/EpisodeLanguage';

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
