import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Alert from 'Components/Alert';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import { kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import AgendaConnector from './Agenda/AgendaConnector';
import * as calendarViews from './calendarViews';
import CalendarDaysConnector from './Day/CalendarDaysConnector';
import DaysOfWeekConnector from './Day/DaysOfWeekConnector';
import CalendarHeaderConnector from './Header/CalendarHeaderConnector';
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
            <Alert kind={kinds.DANGER}>{translate('CalendarLoadError')}</Alert>
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
