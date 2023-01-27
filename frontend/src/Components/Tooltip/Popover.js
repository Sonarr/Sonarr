import PropTypes from 'prop-types';
import React from 'react';
import { tooltipPositions } from 'Helpers/Props';
import Tooltip from './Tooltip';
import styles from './Popover.css';

function Popover(props) {
  const {
    title,
    body,
    ...otherProps
  } = props;

  return (
    <Tooltip
      {...otherProps}
      bodyClassName={styles.tooltipBody}
      tooltip={
        <div>
          <div className={styles.title}>
            {title}
          </div>

          <div className={styles.body}>
            {body}
          </div>
        </div>
      }
    />
  );
}

Popover.propTypes = {
  className: PropTypes.string,
  bodyClassName: PropTypes.string,
  anchor: PropTypes.node.isRequired,
  title: PropTypes.string.isRequired,
  body: PropTypes.oneOfType([PropTypes.string, PropTypes.node]).isRequired,
  position: PropTypes.oneOf(tooltipPositions.all),
  canFlip: PropTypes.bool
};

export default Popover;
