import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import SeriesIndexFooter from './SeriesIndexFooter';

function createMapStateToProps() {
  return createSelector(
    (state) => state.series.items,
    (series) => {
      return {
        series
      };
    }
  );
}

export default connect(createMapStateToProps)(SeriesIndexFooter);
