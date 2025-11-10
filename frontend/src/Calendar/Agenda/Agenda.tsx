import moment from 'moment';
import React from 'react';
import useCalendar from 'Calendar/useCalendar';
import AgendaEvent from './AgendaEvent';
import styles from './Agenda.css';

function Agenda() {
  const { data } = useCalendar();

  return (
    <div className={styles.agenda}>
      {data.map((item, index) => {
        const momentDate = moment(item.airDateUtc);
        const showDate =
          index === 0 ||
          !moment(data[index - 1].airDateUtc).isSame(momentDate, 'day');

        return <AgendaEvent key={item.id} showDate={showDate} {...item} />;
      })}
    </div>
  );
}

export default Agenda;
