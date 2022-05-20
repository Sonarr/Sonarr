import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createEpisodeFileSelector from 'Store/Selectors/createEpisodeFileSelector';
import createQueueItemSelector from 'Store/Selectors/createQueueItemSelector';
import createSeriesSelector from 'Store/Selectors/createSeriesSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import CalendarEvent from './CalendarEvent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.calendar.options,
    createSeriesSelector(),
    createEpisodeFileSelector(),
    createQueueItemSelector(),
    createUISettingsSelector(),
    (calendarOptions, series, episodeFile, queueItem, uiSettings) => {
      return {
        series,
        episodeFile,
        queueItem,
        ...calendarOptions,
        timeFormat: uiSettings.timeFormat,
        colorImpairedMode: uiSettings.enableColorImpairedMode
      };
    }
  );
}

export default connect(createMapStateToProps)(CalendarEvent);
