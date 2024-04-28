import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchHistory, markAsFailed } from 'Store/Actions/historyActions';
import createEpisodeSelector from 'Store/Selectors/createEpisodeSelector';
import createSeriesSelector from 'Store/Selectors/createSeriesSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import SeriesHistoryRow from './SeriesHistoryRow';

function createMapStateToProps() {
  return createSelector(
    createSeriesSelector(),
    createEpisodeSelector(),
    createUISettingsSelector(),
    (series, episode, uiSettings) => {
      return {
        series,
        episode,
        timeFormat: uiSettings.timeFormat,
        shortDateFormat: uiSettings.shortDateFormat
      };
    }
  );
}

const mapDispatchToProps = {
  fetchHistory,
  markAsFailed
};

export default connect(createMapStateToProps, mapDispatchToProps)(SeriesHistoryRow);
