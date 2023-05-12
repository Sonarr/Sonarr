import classNames from 'classnames';
import moment from 'moment';
import React from 'react';
import * as calendarViews from 'Calendar/calendarViews';
import CalendarEventConnector from 'Calendar/Events/CalendarEventConnector';
import CalendarEventGroupConnector from 'Calendar/Events/CalendarEventGroupConnector';
import Series from 'Series/Series';
import CalendarEventGroup, { CalendarEvent } from 'typings/CalendarEventGroup';
import styles from './CalendarDay.css';

interface CalendarDayProps {
  date: string;
  time: string;
  isTodaysDate: boolean;
  events: (CalendarEvent | CalendarEventGroup)[];
  view: string;
  onEventModalOpenToggle(...args: unknown[]): unknown;
}

function CalendarDay(props: CalendarDayProps) {
  const { date, time, isTodaysDate, events, view, onEventModalOpenToggle } =
    props;

  const ref = React.useRef<HTMLDivElement>(null);

  React.useEffect(() => {
    if (isTodaysDate && view === calendarViews.MONTH && ref.current) {
      ref.current.scrollIntoView();
    }
  }, [time, isTodaysDate, view]);

  return (
    <div
      ref={ref}
      className={classNames(
        styles.day,
        view === calendarViews.DAY && styles.isSingleDay
      )}
    >
      {view === calendarViews.MONTH && (
        <div
          className={classNames(
            styles.dayOfMonth,
            isTodaysDate && styles.isToday,
            !moment(date).isSame(moment(time), 'month') &&
              styles.isDifferentMonth
          )}
        >
          {moment(date).date()}
        </div>
      )}
      <div>
        {events.map((event) => {
          if (event.isGroup) {
            return (
              <CalendarEventGroupConnector
                key={event.seriesId}
                {...event}
                onEventModalOpenToggle={onEventModalOpenToggle}
              />
            );
          }

          return (
            <CalendarEventConnector
              key={event.id}
              {...event}
              episodeId={event.id}
              series={event.series as Series}
              airDateUtc={event.airDateUtc as string}
              onEventModalOpenToggle={onEventModalOpenToggle}
            />
          );
        })}
      </div>
    </div>
  );
}

export default CalendarDay;
