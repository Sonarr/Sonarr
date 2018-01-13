import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import DaysOfWeek from './DaysOfWeek';

function createMapStateToProps() {
  return createSelector(
    (state) => state.calendar,
    createUISettingsSelector(),
    (calendar, UiSettings) => {
      return {
        dates: calendar.dates.slice(0, 7),
        view: calendar.view,
        calendarWeekColumnHeader: UiSettings.calendarWeekColumnHeader,
        shortDateFormat: UiSettings.shortDateFormat,
        showRelativeDates: UiSettings.showRelativeDates
      };
    }
  );
}

export default connect(createMapStateToProps)(DaysOfWeek);
