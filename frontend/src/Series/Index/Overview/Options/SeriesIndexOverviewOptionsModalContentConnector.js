import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { setSeriesOverviewOption } from 'Store/Actions/seriesIndexActions';
import SeriesIndexOverviewOptionsModalContent from './SeriesIndexOverviewOptionsModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.seriesIndex,
    (seriesIndex) => {
      return seriesIndex.overviewOptions;
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onChangeOverviewOption(payload) {
      dispatch(setSeriesOverviewOption(payload));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(SeriesIndexOverviewOptionsModalContent);
