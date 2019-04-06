import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { setSeriesSort } from 'Store/Actions/seriesIndexActions';
import SeriesIndexTable from './SeriesIndexTable';

function createMapStateToProps() {
  return createSelector(
    (state) => state.app.dimensions,
    (state) => state.seriesIndex.tableOptions,
    (state) => state.seriesIndex.columns,
    (dimensions, tableOptions, columns) => {
      return {
        isSmallScreen: dimensions.isSmallScreen,
        showBanners: tableOptions.showBanners,
        columns
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onSortPress(sortKey) {
      dispatch(setSeriesSort({ sortKey }));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(SeriesIndexTable);
