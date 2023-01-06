import React from 'react';
import Icon from 'Components/Icon';
import VirtualTableRowCell from 'Components/Table/Cells/TableRowCell';
import { icons } from 'Helpers/Props';
import { getSeriesStatusDetails } from 'Series/SeriesStatus';
import styles from './SeriesStatusCell.css';

interface SeriesStatusCellProps {
  className: string;
  monitored: boolean;
  status: string;
  component?: React.ElementType;
}

function SeriesStatusCell(props: SeriesStatusCellProps) {
  const {
    className,
    monitored,
    status,
    component: Component = VirtualTableRowCell,
    ...otherProps
  } = props;

  const statusDetails = getSeriesStatusDetails(status);

  return (
    <Component className={className} {...otherProps}>
      <Icon
        className={styles.statusIcon}
        name={monitored ? icons.MONITORED : icons.UNMONITORED}
        title={monitored ? 'Series is monitored' : 'Series is unmonitored'}
      />

      <Icon
        className={styles.statusIcon}
        name={statusDetails.icon}
        title={`${statusDetails.title}: ${statusDetails.message}`}
      />
    </Component>
  );
}

export default SeriesStatusCell;
