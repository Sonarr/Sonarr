import PropTypes from 'prop-types';
import React from 'react';
import Icon from 'Components/Icon';
import styles from './LegendIconItem.css';

function LegendIconItem(props) {
  const {
    name,
    icon,
    kind,
    tooltip
  } = props;

  return (
    <div
      className={styles.legendIconItem}
      title={tooltip}
    >
      <Icon
        className={styles.icon}
        name={icon}
        kind={kind}
      />

      {name}
    </div>
  );
}

LegendIconItem.propTypes = {
  name: PropTypes.string.isRequired,
  icon: PropTypes.object.isRequired,
  kind: PropTypes.string.isRequired,
  tooltip: PropTypes.string.isRequired
};

export default LegendIconItem;
