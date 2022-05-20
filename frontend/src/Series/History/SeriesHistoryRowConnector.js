import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchHistory, markAsFailed } from 'Store/Actions/historyActions';
import createEpisodeSelector from 'Store/Selectors/createEpisodeSelector';
import createSeriesSelector from 'Store/Selectors/createSeriesSelector';
import SeriesHistoryRow from './SeriesHistoryRow';

function createMapStateToProps() {
  return createSelector(
    createSeriesSelector(),
    createEpisodeSelector(),
    (series, episode) => {
      return {
        series,
        episode
      };
    }
  );
}

const mapDispatchToProps = {
  fetchHistory,
  markAsFailed
};

export default connect(createMapStateToProps, mapDispatchToProps)(SeriesHistoryRow);
