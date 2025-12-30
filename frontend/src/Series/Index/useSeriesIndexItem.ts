import { maxBy } from 'lodash';
import CommandNames from 'Commands/CommandNames';
import { useCommandExecuting } from 'Commands/useCommands';
import { Season } from 'Series/Series';
import { useSingleSeries } from 'Series/useSeries';
import useSeriesQualityProfile from 'Series/useSeriesQualityProfile';

export function useSeriesIndexItem(seriesId: number) {
  const series = useSingleSeries(seriesId);
  const qualityProfile = useSeriesQualityProfile(series);

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
