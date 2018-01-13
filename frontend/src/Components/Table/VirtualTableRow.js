import PropTypes from 'prop-types';
import React from 'react';
import styles from './VirtualTableRow.css';

function VirtualTableRow(props) {
  const {
    className,
    children,
    style,
    ...otherProps
  } = props;

  return (
    <div
      className={className}
      style={style}
      {...otherProps}
    >
      {children}
    </div>
  );
}

VirtualTableRow.propTypes = {
  className: PropTypes.string.isRequired,
  style: PropTypes.object.isRequired,
  children: PropTypes.node
};

VirtualTableRow.defaultProps = {
  className: styles.row
};

export default VirtualTableRow;
