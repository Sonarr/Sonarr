import moment from 'moment';
import React, { useCallback, useEffect, useRef, useState } from 'react';
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import * as calendarViews from 'Calendar/calendarViews';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import DayOfWeek from './DayOfWeek';
import styles from './DaysOfWeek.css';

function DaysOfWeek() {
  const { dates, view } = useSelector((state: AppState) => state.calendar);
  const { calendarWeekColumnHeader, shortDateFormat, showRelativeDates } =
    useSelector(createUISettingsSelector());

  const updateTimeout = useRef<ReturnType<typeof setTimeout>>();
  const [todaysDate, setTodaysDate] = useState(
    moment().startOf('day').toISOString()
  );

  const scheduleUpdate = useCallback(() => {
    clearTimeout(updateTimeout.current);

    const todaysDate = moment().startOf('day');
    const diff = moment().diff(todaysDate.clone().add(1, 'day'));

    setTodaysDate(todaysDate.toISOString());

    updateTimeout.current = setTimeout(scheduleUpdate, diff);
  }, []);

  useEffect(() => {
    if (view !== calendarViews.AGENDA && view !== calendarViews.MONTH) {
      scheduleUpdate();
    }
  }, [view, scheduleUpdate]);

  if (view === calendarViews.AGENDA) {
    return null;
  }

  return (
    <div className={styles.daysOfWeek}>
      {dates.map((date) => {
        return (
          <DayOfWeek
            key={date}
            date={date}
            view={view}
            isTodaysDate={date === todaysDate}
            calendarWeekColumnHeader={calendarWeekColumnHeader}
            shortDateFormat={shortDateFormat}
            showRelativeDates={showRelativeDates}
          />
        );
      })}
    </div>
  );
}

export default DaysOfWeek;
