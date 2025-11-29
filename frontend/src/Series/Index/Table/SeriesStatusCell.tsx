import React, { useCallback } from 'react';
import Icon from 'Components/Icon';
import MonitorToggleButton from 'Components/MonitorToggleButton';
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
        <Icon
          className={styles.statusIcon}
          name={monitored ? icons.MONITORED : icons.UNMONITORED}
          title={
            monitored
              ? translate('SeriesIsMonitored')
              : translate('SeriesIsUnmonitored')
          }
        />
      )}

      <Icon
        className={styles.statusIcon}
        name={statusDetails.icon}
        title={`${statusDetails.title}: ${statusDetails.message}`}
      />
    </Component>
  );
}

export default SeriesStatusCell;
