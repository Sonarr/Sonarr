import classNames from 'classnames';
import moment from 'moment';
import React, { useCallback, useEffect, useRef, useState } from 'react';
import { useAppValue } from 'App/appStore';
import { useCalendarOption } from 'Calendar/calendarOptionsStore';
import * as calendarViews from 'Calendar/calendarViews';
import getCalendarViewLabel from 'Calendar/getCalendarViewLabel';
import {
  goToNextRange,
  goToPreviousRange,
  useCalendarDates,
} from 'Calendar/useCalendar';
import { useUiSettingsValues } from 'Settings/UI/useUiSettings';
import translate from 'Utilities/String/translate';
import CalendarDay from './CalendarDay';
import DayOfWeek from './DayOfWeek';
import styles from './CalendarDays.css';

function CalendarDays() {
  const view = useCalendarOption('view');
  const dates = useCalendarDates();
  const isSidebarVisible = useAppValue('isSidebarVisible');
  const { calendarWeekColumnHeader, shortDateFormat, showRelativeDates } =
    useUiSettingsValues();

  const updateTimeout = useRef<ReturnType<typeof setTimeout>>();
  const touchStart = useRef<number | null>(null);
  const isEventModalOpen = useRef(false);
  const [todaysDate, setTodaysDate] = useState(
    moment().startOf('day').toISOString()
  );

  const handleEventModalOpenToggle = useCallback((isOpen: boolean) => {
    isEventModalOpen.current = isOpen;
  }, []);

  const scheduleUpdate = useCallback(() => {
    clearTimeout(updateTimeout.current);

    const todaysDate = moment().startOf('day');
    const diff = todaysDate.clone().add(1, 'day').diff(moment());

    setTodaysDate(todaysDate.toISOString());

    updateTimeout.current = setTimeout(scheduleUpdate, diff);
  }, []);

  const handleTouchStart = useCallback(
    (event: React.TouchEvent<HTMLDivElement>) => {
      const touches = event.touches;
      const currentTouch = touches[0].pageX;

      if (touches.length !== 1) {
        return;
      }

      if (currentTouch < 50 || isSidebarVisible || isEventModalOpen.current) {
        return;
      }

      touchStart.current = currentTouch;
    },
    [isSidebarVisible]
  );

  const handleTouchEnd = useCallback(
    (event: React.TouchEvent<HTMLDivElement>) => {
      const touches = event.changedTouches;
      const currentTouch = touches[0].pageX;

      if (!touchStart.current) {
        return;
      }

      if (
        currentTouch > touchStart.current &&
        currentTouch - touchStart.current > 100
      ) {
        goToPreviousRange();
      } else if (
        currentTouch < touchStart.current &&
        touchStart.current - currentTouch > 100
      ) {
        goToNextRange();
      }

      touchStart.current = null;
    },
    []
  );

  const handleTouchCancel = useCallback(() => {
    touchStart.current = null;
  }, []);

  useEffect(() => {
    scheduleUpdate();

    return () => {
      clearTimeout(updateTimeout.current);
    };
  }, [scheduleUpdate]);

  const columnDates = view === calendarViews.MONTH ? dates.slice(0, 7) : dates;
  const rowSize = view === calendarViews.MONTH ? 7 : Math.max(dates.length, 1);
  const rows = [];

  for (let index = 0; index < dates.length; index += rowSize) {
    rows.push(dates.slice(index, index + rowSize));
  }

  return (
    <div
      className={styles.tableContainer}
      onTouchStart={handleTouchStart}
      onTouchEnd={handleTouchEnd}
      onTouchCancel={handleTouchCancel}
    >
      <table
        className={classNames(
          styles.calendarTable,
          styles[view as keyof typeof styles]
        )}
        aria-label={`${translate('Calendar')} - ${getCalendarViewLabel(view)}`}
      >
        <thead>
          <tr>
            {columnDates.map((date) => (
              <DayOfWeek
                key={date}
                date={date}
                view={view}
                isTodaysDate={date === todaysDate}
                calendarWeekColumnHeader={calendarWeekColumnHeader}
                shortDateFormat={shortDateFormat}
                showRelativeDates={showRelativeDates}
              />
            ))}
          </tr>
        </thead>

        <tbody>
          {rows.map((row) => (
            <tr key={row[0]}>
              {row.map((date) => (
                <CalendarDay
                  key={date}
                  date={date}
                  isTodaysDate={date === todaysDate}
                  onEventModalOpenToggle={handleEventModalOpenToggle}
                />
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export default CalendarDays;
