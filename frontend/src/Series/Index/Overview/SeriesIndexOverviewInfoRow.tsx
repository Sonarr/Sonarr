import React from 'react';
import Icon from 'Components/Icon';
import styles from './SeriesIndexOverviewInfoRow.css';

interface SeriesIndexOverviewInfoRowProps {
  title?: string;
  iconName: object;
  label: string;
}

function SeriesIndexOverviewInfoRow(props: SeriesIndexOverviewInfoRowProps) {
  const { title, iconName, label } = props;

  return (
    <div className={styles.infoRow} title={title}>
      <Icon className={styles.icon} name={iconName} size={14} />

      {label}
    </div>
  );
}

export default SeriesIndexOverviewInfoRow;
