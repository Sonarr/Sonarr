import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import ReactDOM from 'react-dom';
import Measure from 'react-measure';
import { Grid, WindowScroller } from 'react-virtualized';
import getIndexOfFirstCharacter from 'Utilities/Array/getIndexOfFirstCharacter';
import hasDifferentItems from 'Utilities/Object/hasDifferentItems';
import dimensions from 'Styles/Variables/dimensions';
import { sortDirections } from 'Helpers/Props';
import SeriesIndexItemConnector from 'Series/Index/SeriesIndexItemConnector';
import SeriesIndexOverview from './SeriesIndexOverview';
import styles from './SeriesIndexOverviews.css';

// Poster container dimensions
const columnPadding = parseInt(dimensions.seriesIndexColumnPadding);
const columnPaddingSmallScreen = parseInt(dimensions.seriesIndexColumnPaddingSmallScreen);
const progressBarHeight = parseInt(dimensions.progressBarSmallHeight);
const detailedProgressBarHeight = parseInt(dimensions.progressBarMediumHeight);

function calculatePosterWidth(posterSize, isSmallScreen) {
  const maxiumPosterWidth = isSmallScreen ? 152 : 162;

  if (posterSize === 'large') {
    return maxiumPosterWidth;
  }

  if (posterSize === 'medium') {
    return Math.floor(maxiumPosterWidth * 0.75);
  }

  return Math.floor(maxiumPosterWidth * 0.5);
}

function calculateRowHeight(posterHeight, sortKey, isSmallScreen, overviewOptions) {
  const {
    detailedProgressBar
  } = overviewOptions;

  const heights = [
    posterHeight,
    detailedProgressBar ? detailedProgressBarHeight : progressBarHeight,
    isSmallScreen ? columnPaddingSmallScreen : columnPadding
  ];

  return heights.reduce((acc, height) => acc + height, 0);
}

function calculatePosterHeight(posterWidth) {
  return Math.ceil((250 / 170) * posterWidth);
}

class SeriesIndexOverviews extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      width: 0,
      columnCount: 1,
      posterWidth: 162,
      posterHeight: 238,
      rowHeight: calculateRowHeight(238, null, props.isSmallScreen, {})
    };

    this._isInitialized = false;
    this._grid = null;
  }

  componentDidMount() {
    this._contentBodyNode = ReactDOM.findDOMNode(this.props.contentBody);
  }

  componentDidUpdate(prevProps) {
    const {
      items,
      filters,
      sortKey,
      sortDirection,
      overviewOptions,
      jumpToCharacter
    } = this.props;

    const itemsChanged = hasDifferentItems(prevProps.items, items);
    const overviewOptionsChanged = !_.isMatch(prevProps.overviewOptions, overviewOptions);

    if (
      prevProps.sortKey !== sortKey ||
      prevProps.overviewOptions !== overviewOptions ||
      itemsChanged
    ) {
      this.calculateGrid();
    }

    if (
      prevProps.filters !== filters ||
      prevProps.sortKey !== sortKey ||
      prevProps.sortDirection !== sortDirection ||
      itemsChanged ||
      overviewOptionsChanged
    ) {
      this._grid.recomputeGridSize();
    }

    if (jumpToCharacter != null && jumpToCharacter !== prevProps.jumpToCharacter) {
      const index = getIndexOfFirstCharacter(items, jumpToCharacter);

      if (index != null) {
        const {
          rowHeight
        } = this.state;

        const scrollTop = rowHeight * index;

        this.props.onScroll({ scrollTop });
      }
    }
  }

  //
  // Control

  scrollToFirstCharacter(character) {
    const items = this.props.items;
    const {
      rowHeight
    } = this.state;

    const index = getIndexOfFirstCharacter(items, character);

    if (index != null) {
      const scrollTop = rowHeight * index;

      this.props.onScroll({ scrollTop });
    }
  }

  setGridRef = (ref) => {
    this._grid = ref;
  }

  calculateGrid = (width = this.state.width, isSmallScreen) => {
    const {
      sortKey,
      overviewOptions
    } = this.props;

    const posterWidth = calculatePosterWidth(overviewOptions.size, isSmallScreen);
    const posterHeight = calculatePosterHeight(posterWidth);
    const rowHeight = calculateRowHeight(posterHeight, sortKey, isSmallScreen, overviewOptions);

    this.setState({
      width,
      posterWidth,
      posterHeight,
      rowHeight
    });
  }

  cellRenderer = ({ key, rowIndex, style }) => {
    const {
      items,
      sortKey,
      overviewOptions,
      showRelativeDates,
      shortDateFormat,
      timeFormat,
      isSmallScreen
    } = this.props;

    const {
      posterWidth,
      posterHeight,
      rowHeight
    } = this.state;

    const series = items[rowIndex];

    if (!series) {
      return null;
    }

    return (
      <SeriesIndexItemConnector
        key={key}
        component={SeriesIndexOverview}
        sortKey={sortKey}
        posterWidth={posterWidth}
        posterHeight={posterHeight}
        rowHeight={rowHeight}
        overviewOptions={overviewOptions}
        showRelativeDates={showRelativeDates}
        shortDateFormat={shortDateFormat}
        timeFormat={timeFormat}
        isSmallScreen={isSmallScreen}
        style={style}
        seriesId={series.id}
        languageProfileId={series.languageProfileId}
        qualityProfileId={series.qualityProfileId}
      />
    );
  }

  //
  // Listeners

  onMeasure = ({ width }) => {
    this.calculateGrid(width, this.props.isSmallScreen);
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
      items,
      scrollTop,
      isSmallScreen,
      onScroll
    } = this.props;

    const {
      width,
      rowHeight
    } = this.state;

    return (
      <Measure onMeasure={this.onMeasure}>
        <WindowScroller
          scrollElement={isSmallScreen ? undefined : this._contentBodyNode}
          onScroll={onScroll}
        >
          {({ height, isScrolling }) => {
            return (
              <Grid
                ref={this.setGridRef}
                className={styles.grid}
                autoHeight={true}
                height={height}
                columnCount={1}
                columnWidth={width}
                rowCount={items.length}
                rowHeight={rowHeight}
                width={width}
                scrollTop={scrollTop}
                overscanRowCount={2}
                cellRenderer={this.cellRenderer}
                onSectionRendered={this.onSectionRendered}
              />
            );
          }
          }
        </WindowScroller>
      </Measure>
    );
  }
}

SeriesIndexOverviews.propTypes = {
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  filters: PropTypes.arrayOf(PropTypes.object).isRequired,
  sortKey: PropTypes.string,
  sortDirection: PropTypes.oneOf(sortDirections.all),
  overviewOptions: PropTypes.object.isRequired,
  scrollTop: PropTypes.number.isRequired,
  jumpToCharacter: PropTypes.string,
  contentBody: PropTypes.object.isRequired,
  showRelativeDates: PropTypes.bool.isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  timeFormat: PropTypes.string.isRequired,
  onRender: PropTypes.func.isRequired,
  onScroll: PropTypes.func.isRequired
};

export default SeriesIndexOverviews;
