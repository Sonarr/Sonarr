import { useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

export type EpisodeEntities =
  | 'calendar'
  | 'episodes'
  | 'interactiveImport.episodes'
  | 'wanted.cutoffUnmet'
  | 'wanted.missing';

function createEpisodeSelector(episodeId?: number) {
  return createSelector(
    (state: AppState) => state.episodes.items,
    (episodes) => {
      return episodes.find(({ id }) => id === episodeId);
    }
  );
}

function createCalendarEpisodeSelector(episodeId?: number) {
  return createSelector(
    (state: AppState) => state.calendar.items,
    (episodes) => {
      return episodes.find(({ id }) => id === episodeId);
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

function useEpisode(
  episodeId: number | undefined,
  episodeEntity: EpisodeEntities
) {
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

  return useSelector(selector(episodeId));
}

export default useEpisode;
