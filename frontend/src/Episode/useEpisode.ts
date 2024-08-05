import { useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

export type EpisodeEntities =
  | 'calendar'
  | 'episodes'
  | 'interactiveImport'
  | 'cutoffUnmet'
  | 'missing';

function createEpisodeSelector(episodeId?: number) {
  return createSelector(
    (state: AppState) => state.episodes.items,
    (episodes) => {
      return episodes.find((e) => e.id === episodeId);
    }
  );
}

function createCalendarEpisodeSelector(episodeId?: number) {
  return createSelector(
    (state: AppState) => state.calendar.items,
    (episodes) => {
      return episodes.find((e) => e.id === episodeId);
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
    default:
      break;
  }

  return useSelector(selector(episodeId));
}

export default useEpisode;
