import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons, sortDirections } from 'Helpers/Props';
import Link from 'Components/Link/Link';
import Icon from 'Components/Icon';
import styles from './VirtualTableHeaderCell.css';

export function headerRenderer(headerProps) {
  const {
    columnData = {},
    dataKey,
    label
  } = headerProps;

  return (
    <VirtualTableHeaderCell
      name={dataKey}
      {...columnData}
    >
      {label}
    </VirtualTableHeaderCell>
  );
}

class VirtualTableHeaderCell extends Component {

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
  }

  //
  // Render

  render() {
    const {
      className,
      name,
      isSortable,
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
          component="div"
          className={className}
          onPress={this.onPress}
          {...otherProps}
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

        <div className={className}>
          {children}
        </div>
    );
  }
}

VirtualTableHeaderCell.propTypes = {
  className: PropTypes.string,
  name: PropTypes.string.isRequired,
  label: PropTypes.oneOfType([PropTypes.string, PropTypes.object]),
  isSortable: PropTypes.bool,
  sortKey: PropTypes.string,
  fixedSortDirection: PropTypes.string,
  sortDirection: PropTypes.string,
  children: PropTypes.node,
  onSortPress: PropTypes.func
};

VirtualTableHeaderCell.defaultProps = {
  className: styles.headerCell,
  isSortable: false
};

export default VirtualTableHeaderCell;
