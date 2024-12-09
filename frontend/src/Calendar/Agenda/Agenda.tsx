import moment from 'moment';
import React from 'react';
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import AgendaEvent from './AgendaEvent';
import styles from './Agenda.css';

function Agenda() {
  const { items } = useSelector((state: AppState) => state.calendar);

  return (
    <div className={styles.agenda}>
      {items.map((item, index) => {
        const momentDate = moment(item.airDateUtc);
        const showDate =
          index === 0 ||
          !moment(items[index - 1].airDateUtc).isSame(momentDate, 'day');

        return <AgendaEvent key={item.id} showDate={showDate} {...item} />;
      })}
    </div>
  );
}

export default Agenda;
