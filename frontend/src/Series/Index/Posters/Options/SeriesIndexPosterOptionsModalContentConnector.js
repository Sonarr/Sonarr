import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { setSeriesPosterOption } from 'Store/Actions/seriesIndexActions';
import SeriesIndexPosterOptionsModalContent from './SeriesIndexPosterOptionsModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.seriesIndex,
    (seriesIndex) => {
      return seriesIndex.posterOptions;
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onChangePosterOption(payload) {
      dispatch(setSeriesPosterOption(payload));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(SeriesIndexPosterOptionsModalContent);
