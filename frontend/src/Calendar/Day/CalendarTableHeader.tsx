import moment from 'moment-timezone';
import React from 'react';
import { useCalendarOption } from 'Calendar/calendarOptionsStore';
import * as calendarViews from 'Calendar/calendarViews';
import { useCalendarDates } from 'Calendar/useCalendar';
import { useUiSettingsValues } from 'Settings/UI/useUiSettings';
import translate from 'Utilities/String/translate';
import styles from './CalendarTableHeader.module.css';

interface CalendarTableHeaderProps {
  todaysDate: string;
}

function CalendarTableHeader({ todaysDate }: CalendarTableHeaderProps) {
  const view = useCalendarOption('view');
  const dates = useCalendarDates();
  const { longDateFormat } = useUiSettingsValues();
  const headerDates = view === calendarViews.MONTH ? dates.slice(0, 7) : dates;

  return (
    <thead>
      <tr className={styles.headerRow}>
        {headerDates.map((date) => {
          const momentDate = moment(date);
          const isTodaysDate =
            view !== calendarViews.MONTH && date === todaysDate;
          const label = momentDate.format(
            view === calendarViews.MONTH ? 'dddd' : longDateFormat
          );

          return (
            <th key={date} className={styles.headerCell} scope="col">
              <time
                className={styles.headerLabel}
                dateTime={momentDate.format('YYYY-MM-DD')}
                aria-current={isTodaysDate ? 'date' : undefined}
              >
                {isTodaysDate ? `${label}, ${translate('Today')}` : label}
              </time>
            </th>
          );
        })}
      </tr>
    </thead>
  );
}

export default CalendarTableHeader;
