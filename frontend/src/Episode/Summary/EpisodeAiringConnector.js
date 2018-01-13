import _ from 'lodash';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import EpisodeAiring from './EpisodeAiring';

function createMapStateToProps() {
  return createSelector(
    createUISettingsSelector(),
    (uiSettings) => {
      return _.pick(uiSettings, [
        'shortDateFormat',
        'showRelativeDates',
        'timeFormat'
      ]);
    }
  );
}

export default connect(createMapStateToProps)(EpisodeAiring);
