import PropTypes from 'prop-types';
import React from 'react';
import styles from './VirtualTableRowCell.css';

function VirtualTableRowCell(props) {
  const {
    className,
    children
  } = props;

  return (
    <div
      className={className}
    >
      {children}
    </div>
  );
}

VirtualTableRowCell.propTypes = {
  className: PropTypes.string.isRequired,
  children: PropTypes.oneOfType([PropTypes.string, PropTypes.node])
};

VirtualTableRowCell.defaultProps = {
  className: styles.cell
};

export default VirtualTableRowCell;
