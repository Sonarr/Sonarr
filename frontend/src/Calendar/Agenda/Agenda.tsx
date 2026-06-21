import React, { Fragment } from 'react';
import useCalendar from 'Calendar/useCalendar';
import { useUiSettingsValues } from 'Settings/UI/useUiSettings';
import { convertToTimezone } from 'Utilities/Date/convertToTimezone';
import AgendaEvent from './AgendaEvent';
import styles from './Agenda.css';

function Agenda() {
  const { data } = useCalendar();
  const { longDateFormat, timeZone } = useUiSettingsValues();

  return (
    <div className={styles.agenda}>
      {data.map((item, index) => {
        const date = convertToTimezone(item.airDateUtc, timeZone);
        const previousDate =
          index > 0
            ? convertToTimezone(data[index - 1].airDateUtc, timeZone)
            : null;
        const showHeader = !previousDate || !date.isSame(previousDate, 'day');

        return (
          <Fragment key={item.id}>
            {showHeader ? (
              <div className={styles.dayHeader}>
                {date.format(longDateFormat)}
              </div>
            ) : null}

            <AgendaEvent {...item} />
          </Fragment>
        );
      })}
    </div>
  );
}

export default Agenda;
