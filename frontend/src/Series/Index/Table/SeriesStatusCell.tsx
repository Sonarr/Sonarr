import React, { useCallback } from 'react';
import Icon from 'Components/Icon';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import StatusIndicator from 'Components/StatusIndicator';
import VirtualTableRowCell from 'Components/Table/Cells/TableRowCell';
import { icons } from 'Helpers/Props';
import { SeriesStatus } from 'Series/Series';
import { getSeriesStatusDetails } from 'Series/SeriesStatus';
import { useToggleSeriesMonitored } from 'Series/useSeries';
import translate from 'Utilities/String/translate';
import styles from './SeriesStatusCell.css';

interface SeriesStatusCellProps {
  className: string;
  seriesId: number;
  monitored: boolean;
  status: SeriesStatus;
  isSelectMode: boolean;
  component?: React.ElementType;
}

function SeriesStatusCell({
  className,
  seriesId,
  monitored,
  status,
  isSelectMode,
  component: Component = VirtualTableRowCell,
  ...otherProps
}: SeriesStatusCellProps) {
  const statusDetails = getSeriesStatusDetails(status);
  const { toggleSeriesMonitored, isTogglingSeriesMonitored } =
    useToggleSeriesMonitored(seriesId);

  const onMonitoredPress = useCallback(() => {
    toggleSeriesMonitored({ monitored: !monitored });
  }, [monitored, toggleSeriesMonitored]);

  return (
    <Component className={className} {...otherProps}>
      {isSelectMode ? (
        <MonitorToggleButton
          className={styles.statusIcon}
          monitored={monitored}
          isSaving={isTogglingSeriesMonitored}
          onPress={onMonitoredPress}
        />
      ) : (
        <StatusIndicator
          className={styles.statusIcon}
          label={
            monitored
              ? translate('SeriesIsMonitored')
              : translate('SeriesIsUnmonitored')
          }
          title={
            monitored
              ? translate('SeriesIsMonitored')
              : translate('SeriesIsUnmonitored')
          }
        >
          <Icon name={monitored ? icons.MONITORED : icons.UNMONITORED} />
        </StatusIndicator>
      )}

      <StatusIndicator
        className={styles.statusIcon}
        label={statusDetails.message}
        title={`${statusDetails.title}: ${statusDetails.message}`}
      >
        <Icon name={statusDetails.icon} />
      </StatusIndicator>
    </Component>
  );
}

export default SeriesStatusCell;
