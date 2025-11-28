import { useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import Episode from './Episode';

function getEpisodes(episodeIds: number[], episodes: Episode[]) {
  return episodeIds.reduce<Episode[]>((acc, id) => {
    const episode = episodes.find((episode) => episode.id === id);

    if (episode) {
      acc.push(episode);
    }

    return acc;
  }, []);
}

function createEpisodeSelector(episodeIds: number[]) {
  return createSelector(
    (state: AppState) => state.episodes.items,
    (episodes) => {
      return getEpisodes(episodeIds, episodes);
    }
  );
}

export default function useEpisodesWithIds(episodeIds: number[]) {
  return useSelector(createEpisodeSelector(episodeIds));
}
