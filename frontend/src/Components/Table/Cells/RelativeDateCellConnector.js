import _ from 'lodash';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import RelativeDateCell from './RelativeDateCell';

function createMapStateToProps() {
  return createSelector(
    createUISettingsSelector(),
    (uiSettings) => {
      return _.pick(uiSettings, [
        'showRelativeDates',
        'shortDateFormat',
        'longDateFormat',
        'timeFormat'
      ]);
    }
  );
}

export default connect(createMapStateToProps, null)(RelativeDateCell);
