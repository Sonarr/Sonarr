import PropTypes from 'prop-types';
import React, { Component } from 'react';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import * as calendarViews from './calendarViews';
import CalendarHeaderConnector from './Header/CalendarHeaderConnector';
import DaysOfWeekConnector from './Day/DaysOfWeekConnector';
import CalendarDaysConnector from './Day/CalendarDaysConnector';
import AgendaConnector from './Agenda/AgendaConnector';
import styles from './Calendar.css';

class Calendar extends Component {

  //
  // Render

  render() {
    const {
      isFetching,
      isPopulated,
      error,
      view
    } = this.props;

    return (
      <div className={styles.calendar}>
        {
          isFetching && !isPopulated &&
            <LoadingIndicator />
        }

        {
          !isFetching && !!error &&
            <div>Unable to load the calendar</div>
        }

        {
          !error && isPopulated && view === calendarViews.AGENDA &&
            <div className={styles.calendarContent}>
              <CalendarHeaderConnector />
              <AgendaConnector />
            </div>
        }

        {
          !error && isPopulated && view !== calendarViews.AGENDA &&
            <div className={styles.calendarContent}>
              <CalendarHeaderConnector />
              <DaysOfWeekConnector />
              <CalendarDaysConnector />
            </div>
        }
      </div>
    );
  }
}

Calendar.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  view: PropTypes.string.isRequired
};

export default Calendar;
