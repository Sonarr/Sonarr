import _ from 'lodash';
import { connect } from 'react-redux';
import { push } from 'react-router-redux';
import { createSelector } from 'reselect';
import createAllSeriesSelector from 'Store/Selectors/createAllSeriesSelector';
import SeriesSearchInput from './SeriesSearchInput';

function createMapStateToProps() {
  return createSelector(
    createAllSeriesSelector(),
    (series) => {
      return {
        series: _.sortBy(series, 'sortTitle')
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onGoToSeries(titleSlug) {
      dispatch(push(`${window.Sonarr.urlBase}/series/${titleSlug}`));
    },

    onGoToAddNewSeries(query) {
      dispatch(push(`${window.Sonarr.urlBase}/add/new?term=${encodeURIComponent(query)}`));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(SeriesSearchInput);
