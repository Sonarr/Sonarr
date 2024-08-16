import React from 'react';
import Icon, { IconProps } from 'Components/Icon';
import styles from './SeriesIndexOverviewInfoRow.css';

interface SeriesIndexOverviewInfoRowProps {
  title?: string;
  iconName: IconProps['name'];
  label: string | null;
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
