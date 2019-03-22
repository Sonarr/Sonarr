import PropTypes from 'prop-types';
import React, { Component } from 'react';
import styles from './TableRowCell.css';

class TableRowCell extends Component {

  //
  // Render

  render() {
    const {
      className,
      children,
      ...otherProps
    } = this.props;

    return (
      <td
        className={className}
        {...otherProps}
      >
        {children}
      </td>
    );
  }
}

TableRowCell.propTypes = {
  className: PropTypes.string.isRequired,
  children: PropTypes.oneOfType([PropTypes.string, PropTypes.node])
};

TableRowCell.defaultProps = {
  className: styles.cell
};

export default TableRowCell;
