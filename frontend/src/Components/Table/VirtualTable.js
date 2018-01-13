import PropTypes from 'prop-types';
import React, { Component } from 'react';
import ReactDOM from 'react-dom';
import Measure from 'react-measure';
import { WindowScroller } from 'react-virtualized';
import { scrollDirections } from 'Helpers/Props';
import Scroller from 'Components/Scroller/Scroller';
import VirtualTableBody from './VirtualTableBody';
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
      width: 0
    };

    this._isInitialized = false;
  }

  componentDidMount() {
    this._contentBodyNode = ReactDOM.findDOMNode(this.props.contentBody);
  }

  componentDidUpdate(prevProps, preState) {
    const scrollIndex = this.props.scrollIndex;

    if (scrollIndex != null && scrollIndex !== prevProps.scrollIndex) {
      const scrollTop = (scrollIndex + 1) * ROW_HEIGHT + 20;

      this.props.onScroll({ scrollTop });
    }
  }

  //
  // Control

  rowGetter = ({ index }) => {
    return this.props.items[index];
  }

  //
  // Listeners

  onMeasure = ({ width }) => {
    this.setState({
      width
    });
  }

  onSectionRendered = () => {
    if (!this._isInitialized && this._contentBodyNode) {
      this.props.onRender();
      this._isInitialized = true;
    }
  }

  //
  // Render

  render() {
    const {
      className,
      items,
      isSmallScreen,
      header,
      headerHeight,
      scrollTop,
      rowRenderer,
      onScroll,
      ...otherProps
    } = this.props;

    const {
      width
    } = this.state;

    return (
      <Measure onMeasure={this.onMeasure}>
        <WindowScroller
          scrollElement={isSmallScreen ? undefined : this._contentBodyNode}
          onScroll={onScroll}
        >
          {({ height, isScrolling }) => {
            return (
              <Scroller
                className={className}
                scrollDirection={scrollDirections.HORIZONTAL}
              >
                {header}

                <VirtualTableBody
                  autoContainerWidth={true}
                  width={width}
                  height={height}
                  headerHeight={height - headerHeight}
                  rowHeight={ROW_HEIGHT}
                  rowCount={items.length}
                  columnCount={1}
                  scrollTop={scrollTop}
                  autoHeight={true}
                  overscanRowCount={2}
                  cellRenderer={rowRenderer}
                  columnWidth={width}
                  overscanIndicesGetter={overscanIndicesGetter}
                  onSectionRendered={this.onSectionRendered}
                  {...otherProps}
                />
              </Scroller>
            );
          }
          }
        </WindowScroller>
      </Measure>
    );
  }
}

VirtualTable.propTypes = {
  className: PropTypes.string.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  scrollTop: PropTypes.number.isRequired,
  scrollIndex: PropTypes.number,
  contentBody: PropTypes.object.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  header: PropTypes.node.isRequired,
  headerHeight: PropTypes.number.isRequired,
  rowRenderer: PropTypes.func.isRequired,
  onRender: PropTypes.func.isRequired,
  onScroll: PropTypes.func.isRequired
};

VirtualTable.defaultProps = {
  className: styles.tableContainer,
  headerHeight: 38,
  onRender: () => {}
};

export default VirtualTable;
