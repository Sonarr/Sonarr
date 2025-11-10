import { QueryKey, useQueryClient } from '@tanstack/react-query';
import { useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import { create } from 'zustand';
import AppState from 'App/State/AppState';
import { CalendarItem } from 'typings/Calendar';

export type EpisodeEntity =
  | 'calendar'
  | 'episodes'
  | 'interactiveImport.episodes'
  | 'wanted.cutoffUnmet'
  | 'wanted.missing';

interface EpisodeQueryKeyStore {
  calendar: QueryKey | null;
}

const episodeQueryKeyStore = create<EpisodeQueryKeyStore>(() => ({
  calendar: null,
}));

function createEpisodeSelector(episodeId?: number) {
  return createSelector(
    (state: AppState) => state.episodes.items,
    (episodes) => {
      return episodes.find(({ id }) => id === episodeId);
    }
  );
}

// No-op...ish
function createCalendarEpisodeSelector(_episodeId?: number) {
  return createSelector(
    (state: AppState) => state.episodes.items,
    (_episodes) => {
      return undefined;
    }
  );
}

function createWantedCutoffUnmetEpisodeSelector(episodeId?: number) {
  return createSelector(
    (state: AppState) => state.wanted.cutoffUnmet.items,
    (episodes) => {
      return episodes.find(({ id }) => id === episodeId);
    }
  );
}

function createWantedMissingEpisodeSelector(episodeId?: number) {
  return createSelector(
    (state: AppState) => state.wanted.missing.items,
    (episodes) => {
      return episodes.find(({ id }) => id === episodeId);
    }
  );
}

export const setEpisodeQueryKey = (
  episodeEntity: EpisodeEntity,
  queryKey: QueryKey | null
) => {
  switch (episodeEntity) {
    case 'calendar':
      episodeQueryKeyStore.setState({ calendar: queryKey });
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
  let selector = createEpisodeSelector;

  switch (episodeEntity) {
    case 'calendar':
      selector = createCalendarEpisodeSelector;
      break;
    case 'wanted.cutoffUnmet':
      selector = createWantedCutoffUnmetEpisodeSelector;
      break;
    case 'wanted.missing':
      selector = createWantedMissingEpisodeSelector;
      break;
    default:
      break;
  }

  const result = useSelector(selector(episodeId));

  if (episodeEntity === 'calendar') {
    const queryKey = episodeQueryKeyStore((state) => state.calendar);

    return queryKey
      ? queryClient
          .getQueryData<CalendarItem[]>(queryKey)
          ?.find((e) => e.id === episodeId)
      : undefined;
  }

  return result;
};

export default useEpisode;
