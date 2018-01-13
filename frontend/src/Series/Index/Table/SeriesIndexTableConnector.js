import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import { setSeriesSort } from 'Store/Actions/seriesIndexActions';
import SeriesIndexTable from './SeriesIndexTable';

function createMapStateToProps() {
  return createSelector(
    (state) => state.app.dimensions,
    createClientSideCollectionSelector('series', 'seriesIndex'),
    (dimensions, series) => {
      return {
        isSmallScreen: dimensions.isSmallScreen,
        ...series
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
