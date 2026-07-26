import classNames from 'classnames';
import React from 'react';
import ScreenReaderOnly from 'Components/ScreenReaderOnly';
import { CalendarStatus } from 'typings/Calendar';
import translate from 'Utilities/String/translate';
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
  const statusTranslationKeys: Record<CalendarStatus, string> = {
    downloaded: 'Downloaded',
    downloading: 'Downloading',
    unmonitored: 'Unmonitored',
    onAir: 'OnAir',
    missing: 'Missing',
    unaired: 'Unaired',
  };
  const statusName = name ?? translate(statusTranslationKeys[status]);

  return (
    <div
      role="listitem"
      className={classNames(
        styles.legendItem,
        styles[status],
        colorImpairedMode && 'colorImpaired',
        fullColorEvents && !isAgendaView && 'fullColor'
      )}
      title={tooltip}
    >
      <span aria-hidden={true}>{statusName}</span>
      <ScreenReaderOnly>
        {statusName}. {tooltip}
      </ScreenReaderOnly>
    </div>
  );
}

export default LegendItem;
