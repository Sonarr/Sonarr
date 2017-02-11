import { createSelector } from 'reselect';
import connectSection from 'Store/connectSection';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import { setSeriesSort } from 'Store/Actions/seriesIndexActions';
import SeriesIndexTable from './SeriesIndexTable';

function createMapStateToProps() {
  return createSelector(
    (state) => state.app.dimensions,
    createClientSideCollectionSelector(),
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

export default connectSection(
  createMapStateToProps,
  createMapDispatchToProps,
  undefined,
  { withRef: true },
  { section: 'series', uiSection: 'seriesIndex' }
)(SeriesIndexTable);
