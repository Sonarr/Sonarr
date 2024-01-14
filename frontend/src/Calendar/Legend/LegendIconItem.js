import classNames from 'classnames';
import PropTypes from 'prop-types';
import React from 'react';
import Icon from 'Components/Icon';
import styles from './LegendIconItem.css';

function LegendIconItem(props) {
  const {
    name,
    fullColorEvents,
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

LegendIconItem.propTypes = {
  name: PropTypes.string.isRequired,
  fullColorEvents: PropTypes.bool.isRequired,
  icon: PropTypes.object.isRequired,
  kind: PropTypes.string.isRequired,
  tooltip: PropTypes.string.isRequired
};

export default LegendIconItem;
