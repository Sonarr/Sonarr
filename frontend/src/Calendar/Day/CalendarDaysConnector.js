import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { gotoCalendarPreviousRange, gotoCalendarNextRange } from 'Store/Actions/calendarActions';
import CalendarDays from './CalendarDays';

function createMapStateToProps() {
  return createSelector(
    (state) => state.calendar,
    (state) => state.app.isSidebarVisible,
    (calendar, isSidebarVisible) => {
      return {
        dates: calendar.dates,
        view: calendar.view,
        isSidebarVisible
      };
    }
  );
}

function createMapDispatchToProps(dispatch) {
  return {
    onNavigatePrevious() {
      dispatch(gotoCalendarPreviousRange());
    },

    onNavigateNext() {
      dispatch(gotoCalendarNextRange());
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(CalendarDays);
