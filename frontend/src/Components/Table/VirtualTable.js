import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { scrollDirections } from 'Helpers/Props';
import Measure from 'Components/Measure';
import Scroller from 'Components/Scroller/Scroller';
import { WindowScroller, Grid } from 'react-virtualized';
import hasDifferentItemsOrOrder from 'Utilities/Object/hasDifferentItemsOrOrder';
import styles from './VirtualTable.css';

function overscanIndicesGetter(options) {
  const {
    cellCount,
    overscanCellsCount,
    startIndex,
    stopIndex
  } = options;

  // The default getter takes the scroll direction into account,
  // but that can cause issues. Ignore the scroll direction and
  // always over return more items.

  const overscanStartIndex = startIndex - overscanCellsCount;
  const overscanStopIndex = stopIndex + overscanCellsCount;

  return {
    overscanStartIndex: Math.max(0, overscanStartIndex),
    overscanStopIndex: Math.min(cellCount - 1, overscanStopIndex)
  };
}

class VirtualTable extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      width: 0
    };

    this._grid = null;
  }

  componentDidUpdate(prevProps, prevState) {
    const {
      items
    } = this.props;

    const {
      width
    } = this.state;

    if (this._grid && (prevState.width !== width || hasDifferentItemsOrOrder(prevProps.items, items))) {
      // recomputeGridSize also forces Grid to discard its cache of rendered cells
      this._grid.recomputeGridSize();
    }
  }

  //
  // Control

  setGridRef = (ref) => {
    this._grid = ref;
  }

  //
  // Listeners

  onMeasure = ({ width }) => {
    this.setState({
      width
    });
  }

  //
  // Render

  render() {
    const {
      isSmallScreen,
      className,
      items,
      scroller,
      header,
      headerHeight,
      rowRenderer,
      rowHeight,
      scrollIndex,
      ...otherProps
    } = this.props;

    const {
      width
    } = this.state;

    const gridStyle = {
      boxSizing: undefined,
      direction: undefined,
      height: undefined,
      position: undefined,
      willChange: undefined,
      overflow: undefined,
      width: undefined
    };

    const containerStyle = {
      position: undefined
    };

    return (
      <WindowScroller
        scrollElement={isSmallScreen ? undefined : scroller}
      >
        {({ height, registerChild, onChildScroll, scrollTop }) => {
          if (!height) {
            return null;
          }

          const finalScrollTop = scrollIndex == null ?
            scrollTop :
            scrollIndex * rowHeight;

          return (
            <Measure
              whitelist={['width']}
              onMeasure={this.onMeasure}
            >
              <Scroller
                className={className}
                scrollDirection={scrollDirections.HORIZONTAL}
              >
                {header}
                <div ref={registerChild}>
                  <Grid
                    ref={this.setGridRef}
                    autoContainerWidth={true}
                    autoHeight={true}
                    autoWidth={true}
                    width={width}
                    height={height}
                    headerHeight={height - headerHeight}
                    rowHeight={rowHeight}
                    rowCount={items.length}
                    columnCount={1}
                    columnWidth={width}
                    scrollTop={finalScrollTop}
                    onScroll={onChildScroll}
                    overscanRowCount={2}
                    cellRenderer={rowRenderer}
                    overscanIndicesGetter={overscanIndicesGetter}
                    scrollToAlignment={'start'}
                    isScrollingOptout={true}
                    className={styles.tableBodyContainer}
                    style={gridStyle}
                    containerStyle={containerStyle}
                    {...otherProps}
                  />
                </div>
              </Scroller>
            </Measure>
          );
        }
        }
      </WindowScroller>
    );
  }
}

VirtualTable.propTypes = {
  isSmallScreen: PropTypes.bool.isRequired,
  className: PropTypes.string.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  scrollIndex: PropTypes.number,
  scroller: PropTypes.instanceOf(Element).isRequired,
  header: PropTypes.node.isRequired,
  headerHeight: PropTypes.number.isRequired,
  rowRenderer: PropTypes.func.isRequired,
  rowHeight: PropTypes.number.isRequired
};

VirtualTable.defaultProps = {
  className: styles.tableContainer,
  headerHeight: 38
};

export default VirtualTable;
