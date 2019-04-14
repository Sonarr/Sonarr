import PropTypes from 'prop-types';
import React from 'react';
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
  title: PropTypes.string.isRequired,
  body: PropTypes.oneOfType([PropTypes.string, PropTypes.node]).isRequired
};

export default Popover;
