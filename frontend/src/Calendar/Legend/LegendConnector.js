import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import Legend from './Legend';

function createMapStateToProps() {
  return createSelector(
    (state) => state.calendar.options,
    (state) => state.calendar.view,
    createUISettingsSelector(),
    (calendarOptions, view, uiSettings) => {
      return {
        ...calendarOptions,
        view,
        colorImpairedMode: uiSettings.enableColorImpairedMode
      };
    }
  );
}

export default connect(createMapStateToProps)(Legend);
