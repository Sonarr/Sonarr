import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { sortDirections } from 'Helpers/Props';
import VirtualTable from 'Components/Table/VirtualTable';
import SeriesIndexItemConnector from 'Series/Index/SeriesIndexItemConnector';
import SeriesIndexHeaderConnector from './SeriesIndexHeaderConnector';
import SeriesIndexRow from './SeriesIndexRow';
import styles from './SeriesIndexTable.css';

class SeriesIndexTable extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._table = null;
  }

  componentDidUpdate(prevProps) {
    const {
      columns,
      filterKey,
      filterValue,
      sortKey,
      sortDirection
    } = this.props;

    if (prevProps.columns !== columns ||
        prevProps.filterKey !== filterKey ||
        prevProps.filterValue !== filterValue ||
        prevProps.sortKey !== sortKey ||
        prevProps.sortDirection !== sortDirection
    ) {
      this._table.forceUpdateGrid();
    }
  }

  //
  // Control

  scrollToFirstCharacter(character) {
    const items = this.props.items;

    const row = _.findIndex(items, (item) => {
      const firstCharacter = item.sortTitle.charAt(0);

      if (character === '#') {
        return !isNaN(firstCharacter);
      }

      return firstCharacter === character;
    });

    if (row != null) {
      this._table.scrollToRow(row);
    }
  }

  setTableRef = (ref) => {
    this._table = ref;
  }

  rowRenderer = ({ key, rowIndex, style }) => {
    const {
      items,
      columns
    } = this.props;

    const series = items[rowIndex];

    return (
      <SeriesIndexItemConnector
        key={key}
        component={SeriesIndexRow}
        style={style}
        columns={columns}
        {...series}
      />
    );
  }

  //
  // Render

  render() {
    const {
      items,
      columns,
      sortKey,
      sortDirection,
      isSmallScreen,
      scrollTop,
      contentBody,
      onSortPress,
      onRender,
      onScroll
    } = this.props;

    return (
      <VirtualTable
        ref={this.setTableRef}
        className={styles.tableContainer}
        items={items}
        scrollTop={scrollTop}
        contentBody={contentBody}
        isSmallScreen={isSmallScreen}
        rowHeight={38}
        overscanRowCount={2}
        rowRenderer={this.rowRenderer}
        header={
          <SeriesIndexHeaderConnector
            columns={columns}
            sortKey={sortKey}
            sortDirection={sortDirection}
            onSortPress={onSortPress}
          />
        }
        onRender={onRender}
        onScroll={onScroll}
      />
    );
  }
}

SeriesIndexTable.propTypes = {
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  filterKey: PropTypes.string,
  filterValue: PropTypes.oneOfType([PropTypes.bool, PropTypes.number, PropTypes.string]),
  sortKey: PropTypes.string,
  sortDirection: PropTypes.oneOf(sortDirections.all),
  scrollTop: PropTypes.number.isRequired,
  contentBody: PropTypes.object.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  onSortPress: PropTypes.func.isRequired,
  onRender: PropTypes.func.isRequired,
  onScroll: PropTypes.func.isRequired
};

export default SeriesIndexTable;
