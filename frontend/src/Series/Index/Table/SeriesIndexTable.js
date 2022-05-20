import PropTypes from 'prop-types';
import React, { Component } from 'react';
import VirtualTable from 'Components/Table/VirtualTable';
import VirtualTableRow from 'Components/Table/VirtualTableRow';
import { sortDirections } from 'Helpers/Props';
import SeriesIndexItemConnector from 'Series/Index/SeriesIndexItemConnector';
import getIndexOfFirstCharacter from 'Utilities/Array/getIndexOfFirstCharacter';
import SeriesIndexHeaderConnector from './SeriesIndexHeaderConnector';
import SeriesIndexRow from './SeriesIndexRow';
import styles from './SeriesIndexTable.css';

class SeriesIndexTable extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      scrollIndex: null
    };
  }

  componentDidUpdate(prevProps) {
    const {
      items,
      jumpToCharacter
    } = this.props;

    if (jumpToCharacter != null && jumpToCharacter !== prevProps.jumpToCharacter) {

      const scrollIndex = getIndexOfFirstCharacter(items, jumpToCharacter);

      if (scrollIndex != null) {
        this.setState({ scrollIndex });
      }
    } else if (jumpToCharacter == null && prevProps.jumpToCharacter != null) {
      this.setState({ scrollIndex: null });
    }
  }

  //
  // Control

  rowRenderer = ({ key, rowIndex, style }) => {
    const {
      items,
      columns,
      showBanners
    } = this.props;

    const series = items[rowIndex];

    return (
      <VirtualTableRow
        key={key}
        style={style}
      >
        <SeriesIndexItemConnector
          key={series.id}
          component={SeriesIndexRow}
          columns={columns}
          seriesId={series.id}
          languageProfileId={series.languageProfileId}
          qualityProfileId={series.qualityProfileId}
          showBanners={showBanners}
        />
      </VirtualTableRow>
    );
  };

  //
  // Render

  render() {
    const {
      items,
      columns,
      sortKey,
      sortDirection,
      showBanners,
      isSmallScreen,
      onSortPress,
      scroller,
      scrollTop
    } = this.props;

    return (
      <VirtualTable
        className={styles.tableContainer}
        items={items}
        scrollIndex={this.state.scrollIndex}
        scrollTop={scrollTop}
        scroller={scroller}
        isSmallScreen={isSmallScreen}
        rowHeight={showBanners ? 70 : 38}
        overscanRowCount={2}
        rowRenderer={this.rowRenderer}
        header={
          <SeriesIndexHeaderConnector
            showBanners={showBanners}
            columns={columns}
            sortKey={sortKey}
            sortDirection={sortDirection}
            onSortPress={onSortPress}
          />
        }
        columns={columns}
      />
    );
  }
}

SeriesIndexTable.propTypes = {
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  sortKey: PropTypes.string,
  sortDirection: PropTypes.oneOf(sortDirections.all),
  showBanners: PropTypes.bool.isRequired,
  jumpToCharacter: PropTypes.string,
  scrollTop: PropTypes.number,
  scroller: PropTypes.instanceOf(Element).isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  onSortPress: PropTypes.func.isRequired
};

export default SeriesIndexTable;
