import { QueryKey, useQueryClient } from '@tanstack/react-query';
import { create } from 'zustand';
import useApiMutation from 'Helpers/Hooks/useApiMutation';
import { PagedQueryResponse } from 'Helpers/Hooks/usePagedApiQuery';
import { CalendarItem } from 'typings/Calendar';
import Episode from './Episode';

export type EpisodeEntity =
  | 'calendar'
  | 'episodes'
  | 'interactiveImport.episodes'
  | 'wanted.cutoffUnmet'
  | 'wanted.missing';

interface EpisodeQueryKeyStore {
  calendar: QueryKey | null;
  episodes: QueryKey | null;
  cutoffUnmet: QueryKey | null;
  missing: QueryKey | null;
}

const episodeQueryKeyStore = create<EpisodeQueryKeyStore>(() => ({
  calendar: null,
  episodes: null,
  cutoffUnmet: null,
  missing: null,
}));

export const getQueryKey = (episodeEntity: EpisodeEntity) => {
  switch (episodeEntity) {
    case 'calendar':
      return episodeQueryKeyStore.getState().calendar;
    case 'episodes':
      return episodeQueryKeyStore.getState().episodes;
    case 'wanted.cutoffUnmet':
      return episodeQueryKeyStore.getState().cutoffUnmet;
    case 'wanted.missing':
      return episodeQueryKeyStore.getState().missing;
    default:
      return null;
  }
};

export const setEpisodeQueryKey = (
  episodeEntity: EpisodeEntity,
  queryKey: QueryKey | null
) => {
  switch (episodeEntity) {
    case 'calendar':
      episodeQueryKeyStore.setState({ calendar: queryKey });
      break;
    case 'episodes':
      episodeQueryKeyStore.setState({ episodes: queryKey });
      break;
    case 'wanted.cutoffUnmet':
      episodeQueryKeyStore.setState({ cutoffUnmet: queryKey });
      break;
    case 'wanted.missing':
      episodeQueryKeyStore.setState({ missing: queryKey });
      break;
    default:
      break;
  }
};

const useEpisode = (
  episodeId: number | undefined,
  episodeEntity: EpisodeEntity
) => {
  const queryClient = useQueryClient();
  const queryKey = getQueryKey(episodeEntity);

  if (episodeEntity === 'calendar') {
    return queryKey
      ? queryClient
          .getQueryData<CalendarItem[]>(queryKey)
          ?.find((e) => e.id === episodeId)
      : undefined;
  }

  if (episodeEntity === 'episodes') {
    return queryKey
      ? queryClient
          .getQueryData<Episode[]>(queryKey)
          ?.find((e) => e.id === episodeId)
      : undefined;
  }

  if (
    episodeEntity === 'wanted.cutoffUnmet' ||
    episodeEntity === 'wanted.missing'
  ) {
    return queryKey
      ? queryClient
          .getQueryData<PagedQueryResponse<Episode>>(queryKey)
          ?.records?.find((e) => e.id === episodeId)
      : undefined;
  }

  return undefined;
};

export default useEpisode;

interface ToggleEpisodesMonitored {
  episodeIds: number[];
  monitored: boolean;
}

export const useToggleEpisodesMonitored = (queryKey: QueryKey) => {
  const queryClient = useQueryClient();

  const { mutate, isPending, variables } = useApiMutation<
    unknown,
    ToggleEpisodesMonitored
  >({
    path: '/episode/monitor',
    method: 'PUT',
    mutationOptions: {
      onSuccess: (_data, variables) => {
        queryClient.setQueryData<Episode[] | undefined>(
          queryKey,
          (oldEpisodes) => {
            if (!oldEpisodes) {
              return oldEpisodes;
            }

            return oldEpisodes.map((oldEpisode) => {
              if (variables.episodeIds.includes(oldEpisode.id)) {
                return {
                  ...oldEpisode,
                  monitored: variables.monitored,
                };
              }

              return oldEpisode;
            });
          }
        );
      },
    },
  });

  return {
    toggleEpisodesMonitored: mutate,
    isToggling: isPending,
    togglingEpisodeIds: variables?.episodeIds ?? [],
    togglingMonitored: variables?.monitored,
  };
};

const DEFAULT_EPISODES: Episode[] = [];

export const useEpisodesWithIds = (episodeIds: number[]) => {
  const queryClient = useQueryClient();
  const queryKey = getQueryKey('episodes');

  return queryKey
    ? queryClient
        .getQueryData<Episode[]>(queryKey)
        ?.filter((e) => episodeIds.includes(e.id)) ?? DEFAULT_EPISODES
    : DEFAULT_EPISODES;
};
