import { maxBy } from 'lodash';
import { useSelector } from 'react-redux';
import CommandNames from 'Commands/CommandNames';
import { useCommandExecuting } from 'Commands/useCommands';
import { Season } from 'Series/Series';
import { useSingleSeries } from 'Series/useSeries';
import createSeriesQualityProfileSelector from 'Store/Selectors/createSeriesQualityProfileSelector';

export function useSeriesIndexItem(seriesId: number) {
  const series = useSingleSeries(seriesId);
  const qualityProfile = useSelector(
    createSeriesQualityProfileSelector(series)
  );

  const isRefreshingSeries = useCommandExecuting(CommandNames.RefreshSeries, {
    seriesIds: [seriesId],
  });

  const isSearchingSeries = useCommandExecuting(CommandNames.SeriesSearch, {
    seriesId,
  });

  const latestSeason: Season | undefined = maxBy(
    series?.seasons || [],
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

export default useSeriesIndexItem;
