import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import * as commandNames from 'Commands/commandNames';
import withScrollPosition from 'Components/withScrollPosition';
import { executeCommand } from 'Store/Actions/commandActions';
import { setSeriesFilter, setSeriesSort, setSeriesTableOption, setSeriesView } from 'Store/Actions/seriesIndexActions';
import scrollPositions from 'Store/scrollPositions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createSeriesClientSideCollectionItemsSelector from 'Store/Selectors/createSeriesClientSideCollectionItemsSelector';
import SeriesIndex from './SeriesIndex';

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
  // Listeners

  onViewSelect = (view) => {
    this.props.dispatchSetSeriesView(view);
  };

  onScroll = ({ scrollTop }) => {
    scrollPositions.seriesIndex = scrollTop;
  };

  //
  // Render

  render() {
    return (
      <SeriesIndex
        {...this.props}
        onViewSelect={this.onViewSelect}
        onScroll={this.onScroll}
      />
    );
  }
}

SeriesIndexConnector.propTypes = {
  isSmallScreen: PropTypes.bool.isRequired,
  view: PropTypes.string.isRequired,
  dispatchSetSeriesView: PropTypes.func.isRequired
};

export default withScrollPosition(
  connect(createMapStateToProps, createMapDispatchToProps)(SeriesIndexConnector),
  'seriesIndex'
);
