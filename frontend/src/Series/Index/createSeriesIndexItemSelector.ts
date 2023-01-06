import { maxBy } from 'lodash';
import { createSelector } from 'reselect';
import { REFRESH_SERIES, SERIES_SEARCH } from 'Commands/commandNames';
import createExecutingCommandsSelector from 'Store/Selectors/createExecutingCommandsSelector';
import createSeriesQualityProfileSelector from 'Store/Selectors/createSeriesQualityProfileSelector';
import createSeriesSelector from 'Store/Selectors/createSeriesSelector';

function createSeriesIndexItemSelector(seriesId: number) {
  return createSelector(
    createSeriesSelector(seriesId),
    createSeriesQualityProfileSelector(seriesId),
    createExecutingCommandsSelector(),
    (series, qualityProfile, executingCommands) => {
      // If a series is deleted this selector may fire before the parent
      // selectors, which will result in an undefined series, if that happens
      // we want to return early here and again in the render function to avoid
      // trying to show a series that has no information available.

      if (!series) {
        return {};
      }

      const isRefreshingSeries = executingCommands.some((command) => {
        return (
          command.name === REFRESH_SERIES && command.body.seriesId === series.id
        );
      });

      const isSearchingSeries = executingCommands.some((command) => {
        return (
          command.name === SERIES_SEARCH && command.body.seriesId === series.id
        );
      });

      const latestSeason = maxBy(
        series.seasons,
        (season) => season.seasonNumber
      );

      return {
        series,
        qualityProfile,
        latestSeason,
        isRefreshingSeries,
        isSearchingSeries,
      };
    }
  );
}

export default createSeriesIndexItemSelector;
