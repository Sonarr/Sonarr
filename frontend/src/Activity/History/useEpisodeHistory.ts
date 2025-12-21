import useApiQuery from 'Helpers/Hooks/useApiQuery';
import History from 'typings/History';

const DEFAULT_HISTORY: History[] = [];

const useEpisodeHistory = (episodeId: number) => {
  const { data, ...result } = useApiQuery<History[]>({
    path: '/history/episode',
    queryParams: {
      episodeId,
    },
  });

  return {
    data: data ?? DEFAULT_HISTORY,
    ...result,
  };
};

export default useEpisodeHistory;
