import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import Legend from './Legend';

function createMapStateToProps() {
  return createSelector(
    (state) => state.calendar.options,
    createUISettingsSelector(),
    (calendarOptions, uiSettings) => {
      return {
        ...calendarOptions,
        colorImpairedMode: uiSettings.enableColorImpairedMode
      };
    }
  );
}

export default connect(createMapStateToProps)(Legend);
