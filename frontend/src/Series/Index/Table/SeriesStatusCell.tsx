import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import Icon from 'Components/Icon';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import VirtualTableRowCell from 'Components/Table/Cells/TableRowCell';
import { icons } from 'Helpers/Props';
import { getSeriesStatusDetails } from 'Series/SeriesStatus';
import { toggleSeriesMonitored } from 'Store/Actions/seriesActions';
import translate from 'Utilities/String/translate';
import styles from './SeriesStatusCell.css';

interface SeriesStatusCellProps {
  className: string;
  seriesId: number;
  monitored: boolean;
  status: string;
  isSelectMode: boolean;
  isSaving: boolean;
  component?: React.ElementType;
}

function SeriesStatusCell(props: SeriesStatusCellProps) {
  const {
    className,
    seriesId,
    monitored,
    status,
    isSelectMode,
    isSaving,
    component: Component = VirtualTableRowCell,
    ...otherProps
  } = props;

  const statusDetails = getSeriesStatusDetails(status);
  const dispatch = useDispatch();

  const onMonitoredPress = useCallback(() => {
    dispatch(toggleSeriesMonitored({ seriesId, monitored: !monitored }));
  }, [seriesId, monitored, dispatch]);

  return (
    <Component className={className} {...otherProps}>
      {isSelectMode ? (
        <MonitorToggleButton
          className={styles.statusIcon}
          monitored={monitored}
          isSaving={isSaving}
          onPress={onMonitoredPress}
        />
      ) : (
        <Icon
          className={styles.statusIcon}
          name={monitored ? icons.MONITORED : icons.UNMONITORED}
          title={
            monitored
              ? translate('Series is monitored')
              : translate('Series is unmonitored')
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
