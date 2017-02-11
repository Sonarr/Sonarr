import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import dimensions from 'Styles/Variables/dimensions';
import createCommandSelector from 'Store/Selectors/createCommandSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import { fetchSeries } from 'Store/Actions/seriesActions';
import scrollPositions from 'Store/scrollPositions';
import { setSeriesSort, setSeriesFilter, setSeriesView } from 'Store/Actions/seriesIndexActions';
import { executeCommand } from 'Store/Actions/commandActions';
import * as commandNames from 'Commands/commandNames';
import withScrollPosition from 'Components/withScrollPosition';
import SeriesIndex from './SeriesIndex';

const POSTERS_PADDING = 15;
const POSTERS_PADDING_SMALL_SCREEN = 5;
const TABLE_PADDING = parseInt(dimensions.pageContentBodyPadding);
const TABLE_PADDING_SMALL_SCREEN = parseInt(dimensions.pageContentBodyPaddingSmallScreen);

// If the scrollTop is greater than zero it needs to be offset
// by the padding so when it is set initially so it is correct
// after React Virtualized takes the padding into account.

function getScrollTop(view, scrollTop, isSmallScreen) {
  if (scrollTop === 0) {
    return 0;
  }

  let padding = isSmallScreen ? TABLE_PADDING_SMALL_SCREEN : TABLE_PADDING;

  if (view === 'posters') {
    padding = isSmallScreen ? POSTERS_PADDING_SMALL_SCREEN : POSTERS_PADDING;
  }

  return scrollTop + padding;
}

function createMapStateToProps() {
  return createSelector(
    (state) => state.series,
    (state) => state.seriesIndex,
    createCommandSelector(commandNames.REFRESH_SERIES),
    createCommandSelector(commandNames.RSS_SYNC),
    createDimensionsSelector(),
    (series, seriesIndex, isRefreshingSeries, isRssSyncExecuting, dimensionsState) => {
      return {
        isRefreshingSeries,
        isRssSyncExecuting,
        isSmallScreen: dimensionsState.isSmallScreen,
        ...series,
        ...seriesIndex
      };
    }
  );
}

const mapDispatchToProps = {
  fetchSeries,
  setSeriesSort,
  setSeriesFilter,
  setSeriesView,
  executeCommand
};

class SeriesIndexConnector extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    const {
      view,
      scrollTop,
      isSmallScreen
    } = props;

    this.state = {
      scrollTop: getScrollTop(view, scrollTop, isSmallScreen)
    };
  }

  componentDidMount() {
    this.props.fetchSeries();
  }

  //
  // Listeners

  onSortSelect = (sortKey) => {
    this.props.setSeriesSort({ sortKey });
  }

  onFilterSelect = (filterKey, filterValue, filterType) => {
    this.props.setSeriesFilter({ filterKey, filterValue, filterType });
  }

  onViewSelect = (view) => {
    // Reset the scroll position before changing the view
    this.setState({ scrollTop: 0 }, () => {
      this.props.setSeriesView({ view });
    });
  }

  onScroll = ({ scrollTop }) => {
    this.setState({
      scrollTop
    }, () => {
      scrollPositions.seriesIndex = scrollTop;
    });
  }

  onRefreshSeriesPress = () => {
    this.props.executeCommand({
      name: commandNames.REFRESH_SERIES
    });
  }

  onRssSyncPress = () => {
    this.props.executeCommand({
      name: commandNames.RSS_SYNC
    });
  }

  //
  // Render

  render() {
    return (
      <SeriesIndex
        {...this.props}
        scrollTop={this.state.scrollTop}
        onSortSelect={this.onSortSelect}
        onFilterSelect={this.onFilterSelect}
        onViewSelect={this.onViewSelect}
        onScroll={this.onScroll}
        onRefreshSeriesPress={this.onRefreshSeriesPress}
        onRssSyncPress={this.onRssSyncPress}
      />
    );
  }
}

SeriesIndexConnector.propTypes = {
  isSmallScreen: PropTypes.bool.isRequired,
  view: PropTypes.string.isRequired,
  scrollTop: PropTypes.number.isRequired,
  fetchSeries: PropTypes.func.isRequired,
  setSeriesSort: PropTypes.func.isRequired,
  setSeriesFilter: PropTypes.func.isRequired,
  setSeriesView: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired
};

export default withScrollPosition(
  connect(createMapStateToProps, mapDispatchToProps)(SeriesIndexConnector),
  'seriesIndex'
);
