import classNames from 'classnames';
import React from 'react';
import { CalendarStatus } from 'typings/Calendar';
import titleCase from 'Utilities/String/titleCase';
import styles from './LegendItem.css';

interface LegendItemProps {
  name?: string;
  status: CalendarStatus;
  tooltip: string;
  isAgendaView: boolean;
  fullColorEvents: boolean;
  colorImpairedMode: boolean;
}

function LegendItem(props: LegendItemProps) {
  const {
    name,
    status,
    tooltip,
    isAgendaView,
    fullColorEvents,
    colorImpairedMode,
  } = props;

  return (
    <div
      className={classNames(
        styles.legendItem,
        styles[status],
        colorImpairedMode && 'colorImpaired',
        fullColorEvents && !isAgendaView && 'fullColor'
      )}
      title={tooltip}
    >
      {name ? name : titleCase(status)}
    </div>
  );
}

export default LegendItem;
