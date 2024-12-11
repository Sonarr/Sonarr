import classNames from 'classnames';
import moment from 'moment';
import React, { useCallback, useEffect, useRef, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import * as calendarViews from 'Calendar/calendarViews';
import {
  gotoCalendarNextRange,
  gotoCalendarPreviousRange,
} from 'Store/Actions/calendarActions';
import CalendarDay from './CalendarDay';
import styles from './CalendarDays.css';

function CalendarDays() {
  const dispatch = useDispatch();
  const { dates, view } = useSelector((state: AppState) => state.calendar);
  const isSidebarVisible = useSelector(
    (state: AppState) => state.app.isSidebarVisible
  );

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
    const diff = moment().diff(todaysDate.clone().add(1, 'day'));

    setTodaysDate(todaysDate.toISOString());

    updateTimeout.current = setTimeout(scheduleUpdate, diff);
  }, []);

  const handleTouchStart = useCallback(
    (event: TouchEvent) => {
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
    (event: TouchEvent) => {
      const touches = event.changedTouches;
      const currentTouch = touches[0].pageX;

      if (!touchStart.current) {
        return;
      }

      if (
        currentTouch > touchStart.current &&
        currentTouch - touchStart.current > 100
      ) {
        dispatch(gotoCalendarPreviousRange());
      } else if (
        currentTouch < touchStart.current &&
        touchStart.current - currentTouch > 100
      ) {
        dispatch(gotoCalendarNextRange());
      }

      touchStart.current = null;
    },
    [dispatch]
  );

  const handleTouchCancel = useCallback(() => {
    touchStart.current = null;
  }, []);

  const handleTouchMove = useCallback(() => {
    if (!touchStart.current) {
      return;
    }
  }, []);

  useEffect(() => {
    if (view === calendarViews.MONTH) {
      scheduleUpdate();
    }
  }, [view, scheduleUpdate]);

  useEffect(() => {
    window.addEventListener('touchstart', handleTouchStart);
    window.addEventListener('touchend', handleTouchEnd);
    window.addEventListener('touchcancel', handleTouchCancel);
    window.addEventListener('touchmove', handleTouchMove);

    return () => {
      window.removeEventListener('touchstart', handleTouchStart);
      window.removeEventListener('touchend', handleTouchEnd);
      window.removeEventListener('touchcancel', handleTouchCancel);
      window.removeEventListener('touchmove', handleTouchMove);
    };
  }, [handleTouchStart, handleTouchEnd, handleTouchCancel, handleTouchMove]);

  return (
    <div
      className={classNames(styles.days, styles[view as keyof typeof styles])}
    >
      {dates.map((date) => {
        return (
          <CalendarDay
            key={date}
            date={date}
            isTodaysDate={date === todaysDate}
            onEventModalOpenToggle={handleEventModalOpenToggle}
          />
        );
      })}
    </div>
  );
}

export default CalendarDays;
