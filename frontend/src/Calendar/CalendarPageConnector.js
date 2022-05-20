import moment from 'moment';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import * as commandNames from 'Commands/commandNames';
import withCurrentPage from 'Components/withCurrentPage';
import { searchMissing, setCalendarDaysCount, setCalendarFilter } from 'Store/Actions/calendarActions';
import { executeCommand } from 'Store/Actions/commandActions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import createSeriesCountSelector from 'Store/Selectors/createSeriesCountSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import { isCommandExecuting } from 'Utilities/Command';
import isBefore from 'Utilities/Date/isBefore';
import CalendarPage from './CalendarPage';

function createMissingEpisodeIdsSelector() {
  return createSelector(
    (state) => state.calendar.start,
    (state) => state.calendar.end,
    (state) => state.calendar.items,
    (state) => state.queue.details.items,
    (start, end, episodes, queueDetails) => {
      return episodes.reduce((acc, episode) => {
        const airDateUtc = episode.airDateUtc;

        if (
          !episode.episodeFileId &&
          moment(airDateUtc).isAfter(start) &&
          moment(airDateUtc).isBefore(end) &&
          isBefore(episode.airDateUtc) &&
          !queueDetails.some((details) => !!details.episode && details.episode.id === episode.id)
        ) {
          acc.push(episode.id);
        }

        return acc;
      }, []);
    }
  );
}

function createIsSearchingSelector() {
  return createSelector(
    (state) => state.calendar.searchMissingCommandId,
    createCommandsSelector(),
    (searchMissingCommandId, commands) => {
      if (searchMissingCommandId == null) {
        return false;
      }

      return isCommandExecuting(commands.find((command) => {
        return command.id === searchMissingCommandId;
      }));
    }
  );
}

function createMapStateToProps() {
  return createSelector(
    (state) => state.calendar.selectedFilterKey,
    (state) => state.calendar.filters,
    createSeriesCountSelector(),
    createUISettingsSelector(),
    createMissingEpisodeIdsSelector(),
    createCommandExecutingSelector(commandNames.RSS_SYNC),
    createIsSearchingSelector(),
    (
      selectedFilterKey,
      filters,
      seriesCount,
      uiSettings,
      missingEpisodeIds,
      isRssSyncExecuting,
      isSearchingForMissing
    ) => {
      return {
        selectedFilterKey,
        filters,
        colorImpairedMode: uiSettings.enableColorImpairedMode,
        hasSeries: !!seriesCount,
        missingEpisodeIds,
        isRssSyncExecuting,
        isSearchingForMissing
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onRssSyncPress() {
      dispatch(executeCommand({
        name: commandNames.RSS_SYNC
      }));
    },

    onSearchMissingPress(episodeIds) {
      dispatch(searchMissing({ episodeIds }));
    },

    onDaysCountChange(dayCount) {
      dispatch(setCalendarDaysCount({ dayCount }));
    },

    onFilterSelect(selectedFilterKey) {
      dispatch(setCalendarFilter({ selectedFilterKey }));
    }
  };
}

export default withCurrentPage(
  connect(createMapStateToProps, createMapDispatchToProps)(CalendarPage)
);
