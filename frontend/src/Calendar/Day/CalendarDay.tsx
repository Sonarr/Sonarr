import classNames from 'classnames';
import moment from 'moment';
import React from 'react';
import { useCalendarOption } from 'Calendar/calendarOptionsStore';
import * as calendarViews from 'Calendar/calendarViews';
import CalendarEvent from 'Calendar/Events/CalendarEvent';
import CalendarEventGroup from 'Calendar/Events/CalendarEventGroup';
import useCalendar, { useCalendarTime } from 'Calendar/useCalendar';
import {
  CalendarEvent as CalendarEventModel,
  CalendarEventGroup as CalendarEventGroupModel,
  CalendarItem,
} from 'typings/Calendar';
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
              <CalendarEventGroup
                key={event.seriesId}
                {...event}
                onEventModalOpenToggle={onEventModalOpenToggle}
              />
            );
          }

          return (
            <CalendarEvent
              key={event.id}
              {...event}
              episodeId={event.id}
              seriesId={event.seriesId}
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
