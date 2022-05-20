import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import createDeepEqualSelector from 'Store/Selectors/createDeepEqualSelector';
import SeriesIndexFooter from './SeriesIndexFooter';

function createUnoptimizedSelector() {
  return createSelector(
    createClientSideCollectionSelector('series', 'seriesIndex'),
    (series) => {
      return series.items.map((s) => {
        const {
          monitored,
          status,
          statistics
        } = s;

        return {
          monitored,
          status,
          statistics
        };
      });
    }
  );
}

function createSeriesSelector() {
  return createDeepEqualSelector(
    createUnoptimizedSelector(),
    (series) => series
  );
}

function createMapStateToProps() {
  return createSelector(
    createSeriesSelector(),
    (series) => {
      return {
        series
      };
    }
  );
}

export default connect(createMapStateToProps)(SeriesIndexFooter);
