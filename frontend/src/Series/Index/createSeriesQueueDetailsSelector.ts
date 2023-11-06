import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

export interface SeriesQueueDetails {
  count: number;
  episodesWithFiles: number;
}

function createSeriesQueueDetailsSelector(
  seriesId: number,
  seasonNumber?: number
) {
  return createSelector(
    (state: AppState) => state.queue.details.items,
    (queueItems) => {
      return queueItems.reduce(
        (acc: SeriesQueueDetails, item) => {
          if (
            item.trackedDownloadState === 'imported' ||
            item.seriesId !== seriesId
          ) {
            return acc;
          }

          if (seasonNumber != null && item.seasonNumber !== seasonNumber) {
            return acc;
          }

          acc.count++;

          if (item.episodeHasFile) {
            acc.episodesWithFiles++;
          }

          return acc;
        },
        {
          count: 0,
          episodesWithFiles: 0,
        }
      );
    }
  );
}

export default createSeriesQueueDetailsSelector;
