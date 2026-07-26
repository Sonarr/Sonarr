import React from 'react';
import useCalendar from 'Calendar/useCalendar';
import ScreenReaderOnly from 'Components/ScreenReaderOnly';
import { useUiSettingsValues } from 'Settings/UI/useUiSettings';
import { CalendarItem } from 'typings/Calendar';
import { convertToTimezone } from 'Utilities/Date/convertToTimezone';
import translate from 'Utilities/String/translate';
import AgendaEvent from './AgendaEvent';
import styles from './Agenda.css';

function Agenda() {
  const { data } = useCalendar();
  const { longDateFormat, timeZone } = useUiSettingsValues();

  const dateGroups = data.reduce<
    { date: string; label: string; items: CalendarItem[] }[]
  >((groups, item) => {
    const date = convertToTimezone(item.airDateUtc, timeZone);
    const dateKey = date.format('YYYY-MM-DD');
    const lastGroup = groups[groups.length - 1];

    if (!lastGroup || lastGroup.date !== dateKey) {
      groups.push({
        date: dateKey,
        label: date.format(longDateFormat),
        items: [item],
      });
    } else {
      lastGroup.items.push(item);
    }

    return groups;
  }, []);

  return (
    <section className={styles.agenda} aria-labelledby="calendar-agenda-title">
      <ScreenReaderOnly id="calendar-agenda-title">
        {translate('Agenda')}
      </ScreenReaderOnly>

      {dateGroups.length ? (
        dateGroups.map((group) => {
          const headingId = `calendar-agenda-${group.date}`;

          return (
            <section
              key={group.date}
              className={styles.day}
              aria-labelledby={headingId}
            >
              <h3 id={headingId} className={styles.dayHeading}>
                <time dateTime={group.date}>{group.label}</time>
              </h3>

              <ul className={styles.eventList}>
                {group.items.map((item) => (
                  <li key={item.id}>
                    <AgendaEvent {...item} />
                  </li>
                ))}
              </ul>
            </section>
          );
        })
      ) : (
        <div role="status">{translate('CalendarNoEpisodesInRange')}</div>
      )}
    </section>
  );
}

export default Agenda;
