import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createSeriesClientSideCollectionItemsSelector from 'Store/Selectors/createSeriesClientSideCollectionItemsSelector';
import dimensions from 'Styles/Variables/dimensions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import scrollPositions from 'Store/scrollPositions';
import { setSeriesSort, setSeriesFilter, setSeriesView, setSeriesTableOption } from 'Store/Actions/seriesIndexActions';
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
    createSeriesClientSideCollectionItemsSelector('seriesIndex'),
    createCommandExecutingSelector(commandNames.REFRESH_SERIES),
    createCommandExecutingSelector(commandNames.RSS_SYNC),
    createDimensionsSelector(),
    (
      series,
      isRefreshingSeries,
      isRssSyncExecuting,
      dimensionsState
    ) => {
      return {
        ...series,
        isRefreshingSeries,
        isRssSyncExecuting,
        isSmallScreen: dimensionsState.isSmallScreen
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onTableOptionChange(payload) {
      dispatch(setSeriesTableOption(payload));
    },

    onSortSelect(sortKey) {
      dispatch(setSeriesSort({ sortKey }));
    },

    onFilterSelect(selectedFilterKey) {
      dispatch(setSeriesFilter({ selectedFilterKey }));
    },

    dispatchSetSeriesView(view) {
      dispatch(setSeriesView({ view }));
    },

    onRefreshSeriesPress() {
      dispatch(executeCommand({
        name: commandNames.REFRESH_SERIES
      }));
    },

    onRssSyncPress() {
      dispatch(executeCommand({
        name: commandNames.RSS_SYNC
      }));
    }
  };
}

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

  //
  // Listeners

  onViewSelect = (view) => {
    // Reset the scroll position before changing the view
    this.setState({ scrollTop: 0 }, () => {
      this.props.dispatchSetSeriesView(view);
    });
  }

  onScroll = ({ scrollTop }) => {
    this.setState({
      scrollTop
    }, () => {
      scrollPositions.seriesIndex = scrollTop;
    });
  }

  //
  // Render

  render() {
    return (
      <SeriesIndex
        {...this.props}
        scrollTop={this.state.scrollTop}
        onViewSelect={this.onViewSelect}
        onScroll={this.onScroll}
      />
    );
  }
}

SeriesIndexConnector.propTypes = {
  isSmallScreen: PropTypes.bool.isRequired,
  view: PropTypes.string.isRequired,
  scrollTop: PropTypes.number.isRequired,
  dispatchSetSeriesView: PropTypes.func.isRequired
};

export default withScrollPosition(
  connect(createMapStateToProps, createMapDispatchToProps)(SeriesIndexConnector),
  'seriesIndex'
);
