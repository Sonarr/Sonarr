import PropTypes from 'prop-types';
import React from 'react';
import Link from 'Components/Link/Link';
import TableRowCell from './TableRowCell';
import styles from './TableRowCellButton.css';

function TableRowCellButton({ className, ...otherProps }) {
  return (
    <Link
      className={className}
      component={TableRowCell}
      {...otherProps}
    />
  );
}

TableRowCellButton.propTypes = {
  className: PropTypes.string.isRequired
};

TableRowCellButton.defaultProps = {
  className: styles.cell
};

export default TableRowCellButton;
