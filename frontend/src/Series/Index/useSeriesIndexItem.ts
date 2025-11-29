import { maxBy } from 'lodash';
import { useMemo } from 'react';
import { useSelector } from 'react-redux';
import Command from 'Commands/Command';
import { REFRESH_SERIES, SERIES_SEARCH } from 'Commands/commandNames';
import { useSingleSeries } from 'Series/useSeries';
import createExecutingCommandsSelector from 'Store/Selectors/createExecutingCommandsSelector';
import createSeriesQualityProfileSelector from 'Store/Selectors/createSeriesQualityProfileSelector';

function useSeriesIndexItem(seriesId: number) {
  const series = useSingleSeries(seriesId);
  const qualityProfile = useSelector(
    createSeriesQualityProfileSelector(series)
  );
  const executingCommands: Command[] = useSelector(
    createExecutingCommandsSelector()
  );

  return useMemo(() => {
    if (!series) {
      throw new Error('Series not found');
    }

    const isRefreshingSeries = executingCommands.some((command) => {
      return (
        command.name === REFRESH_SERIES &&
        command.body.seriesIds?.includes(series.id)
      );
    });

    const isSearchingSeries = executingCommands.some((command) => {
      return (
        command.name === SERIES_SEARCH && command.body.seriesId === seriesId
      );
    });

    const latestSeason = maxBy(series.seasons, (season) => season.seasonNumber);

    return {
      series,
      qualityProfile,
      latestSeason,
      isRefreshingSeries,
      isSearchingSeries,
    };
  }, [series, qualityProfile, executingCommands, seriesId]);
}

export default useSeriesIndexItem;
