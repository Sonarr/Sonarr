import { FontAwesomeIconProps } from '@fortawesome/react-fontawesome';
import classNames from 'classnames';
import React from 'react';
import Icon, { IconProps } from 'Components/Icon';
import styles from './LegendIconItem.css';

interface LegendIconItemProps extends Pick<IconProps, 'kind'> {
  name: string;
  fullColorEvents: boolean;
  icon: FontAwesomeIconProps['icon'];
  tooltip: string;
}

function LegendIconItem(props: LegendIconItemProps) {
  const { name, fullColorEvents, icon, kind, tooltip } = props;

  return (
    <div className={styles.legendIconItem} title={tooltip}>
      <Icon
        className={classNames(
          styles.icon,
          fullColorEvents && 'fullColorEvents'
        )}
        name={icon}
        kind={kind}
      />

      {name}
    </div>
  );
}

export default LegendIconItem;
