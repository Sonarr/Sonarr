import { useMemo } from 'react';
import { useSelect } from 'App/Select/SelectContext';
import Series from 'Series/Series';
import useSeries from 'Series/useSeries';
import sortByProp from 'Utilities/Array/sortByProp';

function useSelectedSeriesStats() {
  const { data: allSeries } = useSeries();
  const { useSelectedIds } = useSelect<Series>();
  const seriesIds = useSelectedIds();

  const series = useMemo((): Series[] => {
    const seriesList = seriesIds.map((id) => {
      return allSeries.find((s) => s.id === id);
    }) as Series[];

    return seriesList.sort(sortByProp('sortTitle'));
  }, [allSeries, seriesIds]);

  const { totalEpisodeFileCount, totalSizeOnDisk } = useMemo(() => {
    return series.reduce(
      (acc, { statistics = {} }) => {
        const { episodeFileCount = 0, sizeOnDisk = 0 } = statistics;

        acc.totalEpisodeFileCount += episodeFileCount;
        acc.totalSizeOnDisk += sizeOnDisk;

        return acc;
      },
      {
        totalEpisodeFileCount: 0,
        totalSizeOnDisk: 0,
      }
    );
  }, [series]);

  return {
    series,
    seriesIds,
    totalEpisodeFileCount,
    totalSizeOnDisk,
  };
}

export default useSelectedSeriesStats;
