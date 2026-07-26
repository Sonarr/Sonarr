import classNames from 'classnames';
import moment from 'moment';
import React from 'react';
import { useCalendarOption } from 'Calendar/calendarOptionsStore';
import * as calendarViews from 'Calendar/calendarViews';
import CalendarEvent from 'Calendar/Events/CalendarEvent';
import CalendarEventGroup from 'Calendar/Events/CalendarEventGroup';
import useCalendar, { useCalendarTime } from 'Calendar/useCalendar';
import ScreenReaderOnly from 'Components/ScreenReaderOnly';
import {
  CalendarEvent as CalendarEventModel,
  CalendarEventGroup as CalendarEventGroupModel,
  CalendarItem,
} from 'typings/Calendar';
import translate from 'Utilities/String/translate';
import styles from './CalendarDay.css';

function sort(items: (CalendarEventModel | CalendarEventGroupModel)[]) {
  return items.sort((a, b) => {
    const aDate = a.isGroup
      ? moment(a.events[0].airDateUtc).unix()
      : moment(a.airDateUtc).unix();

    const bDate = b.isGroup
      ? moment(b.events[0].airDateUtc).unix()
      : moment(b.airDateUtc).unix();

    return aDate - bDate;
  });
}

const useCalendarEvents = (date: string) => {
  const { data } = useCalendar();
  const collapseMultipleEpisodes = useCalendarOption(
    'collapseMultipleEpisodes'
  );

  const momentDate = moment(date);

  const filtered = data.filter((item) => {
    return momentDate.isSame(moment(item.airDateUtc), 'day');
  });

  if (!collapseMultipleEpisodes) {
    return sort(
      filtered.map((item) => ({
        isGroup: false,
        ...item,
      }))
    );
  }

  const groupedObject = Object.groupBy(
    filtered,
    (item: CalendarItem) => `${item.seriesId}-${item.seasonNumber}`
  );

  const grouped = Object.entries(groupedObject).reduce<
    (CalendarEventModel | CalendarEventGroupModel)[]
  >((acc, [, events]) => {
    if (!events) {
      return acc;
    }

    if (events.length === 1) {
      acc.push({
        isGroup: false,
        ...events[0],
      });
    } else {
      acc.push({
        isGroup: true,
        seriesId: events[0].seriesId,
        seasonNumber: events[0].seasonNumber,
        episodeIds: events.map((event) => event.id),
        events: events.sort(
          (a, b) => moment(a.airDateUtc).unix() - moment(b.airDateUtc).unix()
        ),
      });
    }

    return acc;
  }, []);

  return sort(grouped);
};

interface CalendarDayProps {
  date: string;
  isTodaysDate: boolean;
  onEventModalOpenToggle(isOpen: boolean): unknown;
}

function CalendarDay({
  date,
  isTodaysDate,
  onEventModalOpenToggle,
}: CalendarDayProps) {
  const view = useCalendarOption('view');
  const time = useCalendarTime();
  const events = useCalendarEvents(date);

  const ref = React.useRef<HTMLTableCellElement>(null);
  const momentDate = moment(date);
  const fullDate = momentDate.format('dddd, MMMM D, YYYY');
  const isDifferentMonth = !momentDate.isSame(moment(time), 'month');

  return (
    <td
      ref={ref}
      className={classNames(
        styles.day,
        view === calendarViews.DAY && styles.isSingleDay
      )}
    >
      <h3
        className={
          view === calendarViews.MONTH
            ? classNames(
                styles.dayOfMonth,
                isTodaysDate && styles.isToday,
                isDifferentMonth && styles.isDifferentMonth
              )
            : styles.dateHeading
        }
      >
        <time
          dateTime={momentDate.format('YYYY-MM-DD')}
          aria-current={isTodaysDate ? 'date' : undefined}
          aria-label={fullDate}
        >
          {view === calendarViews.MONTH ? momentDate.date() : fullDate}
        </time>

        {isTodaysDate ? (
          <ScreenReaderOnly>, {translate('Today')}</ScreenReaderOnly>
        ) : null}
      </h3>

      {events.length ? (
        <ul className={styles.eventList}>
          {events.map((event) => {
            if (event.isGroup) {
              return (
                <li key={`${event.seriesId}-${event.seasonNumber}`}>
                  <CalendarEventGroup
                    {...event}
                    onEventModalOpenToggle={onEventModalOpenToggle}
                  />
                </li>
              );
            }

            return (
              <li key={event.id}>
                <CalendarEvent
                  {...event}
                  episodeId={event.id}
                  seriesId={event.seriesId}
                  airDateUtc={event.airDateUtc as string}
                  onEventModalOpenToggle={onEventModalOpenToggle}
                />
              </li>
            );
          })}
        </ul>
      ) : (
        <ScreenReaderOnly>{translate('CalendarNoEpisodes')}</ScreenReaderOnly>
      )}
    </td>
  );
}

export default CalendarDay;
