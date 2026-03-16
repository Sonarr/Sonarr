import useApiQuery from 'Helpers/Hooks/useApiQuery';
import { EpisodeOrderType } from 'Series/Series';

export interface EpisodeOrderingOption {
  type: EpisodeOrderType;
  name: string;
}

const DEFAULT_ORDERINGS: EpisodeOrderingOption[] = [];

const useAvailableOrderings = (seriesId: number, enabled = true) => {
  const { data, ...result } = useApiQuery<EpisodeOrderingOption[]>({
    path: `/series/${seriesId}/availableOrderings`,
    queryOptions: {
      staleTime: 24 * 60 * 60 * 1000, // 24 hours — matches TVDB cache TTL
      gcTime: Infinity,
      enabled,
    },
  });

  return {
    ...result,
    data: data ?? DEFAULT_ORDERINGS,
  };
};

export default useAvailableOrderings;
