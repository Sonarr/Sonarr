import PropTypes from 'prop-types';
import React from 'react';
import styles from './TableRow.css';

function TableRow(props) {
  const {
    className,
    children,
    ...otherProps
  } = props;

  return (
    <tr
      className={className}
      {...otherProps}
    >
      {children}
    </tr>
  );
}

TableRow.propTypes = {
  className: PropTypes.string.isRequired,
  children: PropTypes.node
};

TableRow.defaultProps = {
  className: styles.row
};

export default TableRow;
