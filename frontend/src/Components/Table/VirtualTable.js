import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { Grid, WindowScroller } from 'react-virtualized';
import Measure from 'Components/Measure';
import Scroller from 'Components/Scroller/Scroller';
import { scrollDirections } from 'Helpers/Props';
import hasDifferentItemsOrOrder from 'Utilities/Object/hasDifferentItemsOrOrder';
import styles from './VirtualTable.css';

const ROW_HEIGHT = 38;

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
      width: 0,
      scrollRestored: false
    };

    this._grid = null;
  }

  componentDidUpdate(prevProps, prevState) {
    const {
      items,
      scrollIndex,
      scrollTop
    } = this.props;

    const {
      width,
      scrollRestored
    } = this.state;

    if (this._grid && (prevState.width !== width || hasDifferentItemsOrOrder(prevProps.items, items))) {
      // recomputeGridSize also forces Grid to discard its cache of rendered cells
      this._grid.recomputeGridSize();
    }

    if (this._grid && scrollTop !== undefined && scrollTop !== 0 && !scrollRestored) {
      this.setState({ scrollRestored: true });
      this._grid.scrollToPosition({ scrollTop });
    }

    if (scrollIndex != null && scrollIndex !== prevProps.scrollIndex) {
      this._grid.scrollToCell({
        rowIndex: scrollIndex,
        columnIndex: 0
      });
    }
  }

  //
  // Control

  setGridRef = (ref) => {
    this._grid = ref;
  };

  //
  // Listeners

  onMeasure = ({ width }) => {
    this.setState({
      width
    });
  };

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
      rowHeight,
      rowRenderer,
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
                    {...otherProps}
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
                    scrollTop={scrollTop}
                    onScroll={onChildScroll}
                    overscanRowCount={2}
                    cellRenderer={rowRenderer}
                    overscanIndicesGetter={overscanIndicesGetter}
                    scrollToAlignment={'start'}
                    isScrollingOptout={true}
                    className={styles.tableBodyContainer}
                    style={gridStyle}
                    containerStyle={containerStyle}
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
  scrollTop: PropTypes.number,
  scroller: PropTypes.instanceOf(Element).isRequired,
  header: PropTypes.node.isRequired,
  headerHeight: PropTypes.number.isRequired,
  rowRenderer: PropTypes.func.isRequired,
  rowHeight: PropTypes.number.isRequired
};

VirtualTable.defaultProps = {
  className: styles.tableContainer,
  headerHeight: 38,
  rowHeight: ROW_HEIGHT
};

export default VirtualTable;
