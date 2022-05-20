/* eslint max-params: 0 */
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createEpisodeFileSelector from 'Store/Selectors/createEpisodeFileSelector';
import createSeriesSelector from 'Store/Selectors/createSeriesSelector';
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
        releaseGroup: episodeFile ? episodeFile.releaseGroup : null,
        alternateTitles: series.alternateTitles
      };
    }
  );
}
export default connect(createMapStateToProps)(EpisodeRow);
