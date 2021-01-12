/* eslint max-params: 0 */
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createSeriesSelector from 'Store/Selectors/createSeriesSelector';
import createEpisodeFileSelector from 'Store/Selectors/createEpisodeFileSelector';
import EpisodeRow from './EpisodeRow';

function createMapStateToProps() {
  return createSelector(
    createSeriesSelector(),
    createEpisodeFileSelector(),
    (series = {}, episodeFile) => {
      return {
        useSceneNumbering: series.useSceneNumbering,
        seriesMonitored: series.monitored,
        seriesType: series.seriesType,
        episodeFilePath: episodeFile ? episodeFile.path : null,
        episodeFileRelativePath: episodeFile ? episodeFile.relativePath : null,
        episodeFileSize: episodeFile ? episodeFile.size : null,
        alternateTitles: series.alternateTitles
      };
    }
  );
}
export default connect(createMapStateToProps)(EpisodeRow);
