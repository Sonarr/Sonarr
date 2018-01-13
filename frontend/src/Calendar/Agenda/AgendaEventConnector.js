import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createSeriesSelector from 'Store/Selectors/createSeriesSelector';
import createQueueItemSelector from 'Store/Selectors/createQueueItemSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import AgendaEvent from './AgendaEvent';

function createMapStateToProps() {
  return createSelector(
    createSeriesSelector(),
    createQueueItemSelector(),
    createUISettingsSelector(),
    (series, queueItem, uiSettings) => {
      return {
        series,
        queueItem,
        timeFormat: uiSettings.timeFormat,
        longDateFormat: uiSettings.longDateFormat
      };
    }
  );
}

export default connect(createMapStateToProps)(AgendaEvent);
