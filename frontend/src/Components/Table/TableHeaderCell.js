import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import { icons, sortDirections } from 'Helpers/Props';
import styles from './TableHeaderCell.css';

class TableHeaderCell extends Component {

  //
  // Listeners

  onPress = () => {
    const {
      name,
      fixedSortDirection
    } = this.props;

    if (fixedSortDirection) {
      this.props.onSortPress(name, fixedSortDirection);
    } else {
      this.props.onSortPress(name);
    }
  };

  //
  // Render

  render() {
    const {
      className,
      name,
      label,
      columnLabel,
      isSortable,
      isVisible,
      isModifiable,
      sortKey,
      sortDirection,
      fixedSortDirection,
      children,
      onSortPress,
      ...otherProps
    } = this.props;

    const isSorting = isSortable && sortKey === name;
    const sortIcon = sortDirection === sortDirections.ASCENDING ?
      icons.SORT_ASCENDING :
      icons.SORT_DESCENDING;

    return (
      isSortable ?
        <Link
          {...otherProps}
          component="th"
          className={className}
          label={typeof label === 'function' ? label() : label}
          title={typeof columnLabel === 'function' ? columnLabel() : columnLabel}
          onPress={this.onPress}
        >
          {children}

          {
            isSorting &&
              <Icon
                name={sortIcon}
                className={styles.sortIcon}
              />
          }
        </Link> :

        <th className={className}>
          {children}
        </th>
    );
  }
}

TableHeaderCell.propTypes = {
  className: PropTypes.string,
  name: PropTypes.string.isRequired,
  label: PropTypes.oneOfType([PropTypes.string, PropTypes.func, PropTypes.node]),
  columnLabel: PropTypes.oneOfType([PropTypes.string, PropTypes.func]),
  isSortable: PropTypes.bool,
  isVisible: PropTypes.bool,
  isModifiable: PropTypes.bool,
  sortKey: PropTypes.string,
  fixedSortDirection: PropTypes.string,
  sortDirection: PropTypes.string,
  children: PropTypes.node,
  onSortPress: PropTypes.func
};

TableHeaderCell.defaultProps = {
  className: styles.headerCell,
  isSortable: false
};

export default TableHeaderCell;
