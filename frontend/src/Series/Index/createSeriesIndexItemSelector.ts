import { maxBy } from 'lodash';
import { createSelector } from 'reselect';
import Command from 'Commands/Command';
import { REFRESH_SERIES, SERIES_SEARCH } from 'Commands/commandNames';
import Series from 'Series/Series';
import createExecutingCommandsSelector from 'Store/Selectors/createExecutingCommandsSelector';
import createSeriesQualityProfileSelector from 'Store/Selectors/createSeriesQualityProfileSelector';
import { createSeriesSelectorForHook } from 'Store/Selectors/createSeriesSelector';

function createSeriesIndexItemSelector(seriesId: number) {
  return createSelector(
    createSeriesSelectorForHook(seriesId),
    createSeriesQualityProfileSelector(seriesId),
    createExecutingCommandsSelector(),
    (series: Series, qualityProfile, executingCommands: Command[]) => {
      const isRefreshingSeries = executingCommands.some((command) => {
        return (
          command.name === REFRESH_SERIES && command.body.seriesId === seriesId
        );
      });

      const isSearchingSeries = executingCommands.some((command) => {
        return (
          command.name === SERIES_SEARCH && command.body.seriesId === seriesId
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
