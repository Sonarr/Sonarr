import useApiQuery from 'Helpers/Hooks/useApiQuery';
import History from 'typings/History';

const DEFAULT_HISTORY: History[] = [];

const useSeriesHistory = (
  seriesId: number,
  seasonNumber: number | undefined
) => {
  const { data, ...result } = useApiQuery<History[]>({
    path: '/history/series',
    queryParams: {
      seriesId,
      seasonNumber,
    },
  });

  return {
    data: data ?? DEFAULT_HISTORY,
    ...result,
  };
};

export default useSeriesHistory;
