import useApiQuery from 'Helpers/Hooks/useApiQuery';
import Episode from './Episode';

const DEFAULT_EPISODES: Episode[] = [];

interface SeriesEpisodes {
  seriesId: number;
}

interface SeasonEpisodes {
  seriesId: number | undefined;
  seasonNumber: number | undefined;
}

interface EpisodeIds {
  episodeIds: number[];
}

interface EpisodeFileId {
  episodeFileId: number;
}

export type EpisodeFilter =
  | SeriesEpisodes
  | SeasonEpisodes
  | EpisodeIds
  | EpisodeFileId;

const useEpisodes = (params: EpisodeFilter) => {
  const result = useApiQuery<Episode[]>({
    path: '/episode',
    queryParams: { ...params },
    queryOptions: {
      enabled:
        ('seriesId' in params && params.seriesId !== undefined) ||
        ('episodeIds' in params && params.episodeIds?.length > 0) ||
        ('episodeFileId' in params && params.episodeFileId !== undefined),
    },
  });

  return {
    ...result,
    data: result.data ?? DEFAULT_EPISODES,
  };
};

export default useEpisodes;
