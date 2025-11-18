import React, {
  createContext,
  PropsWithChildren,
  useContext,
  useMemo,
} from 'react';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import Queue from 'typings/Queue';

interface EpisodeDetails {
  episodeIds: number[];
}

interface SeriesDetails {
  seriesId: number;
}

interface AllDetails {
  all: boolean;
}

type QueueDetailsFilter = AllDetails | EpisodeDetails | SeriesDetails;

const QueueDetailsContext = createContext<Queue[] | undefined>(undefined);

export default function QueueDetailsProvider({
  children,
  ...filter
}: PropsWithChildren<QueueDetailsFilter>) {
  const { data } = useApiQuery<Queue[]>({
    path: '/queue/details',
    queryParams: { ...filter },
    queryOptions: {
      enabled: Object.keys(filter).length > 0,
    },
  });

  return (
    <QueueDetailsContext.Provider value={data}>
      {children}
    </QueueDetailsContext.Provider>
  );
}

export function useQueueItemForEpisode(episodeId: number) {
  const queue = useContext(QueueDetailsContext);

  return useMemo(() => {
    return queue?.find((item) => item.episodeIds.includes(episodeId));
  }, [episodeId, queue]);
}

export function useIsDownloadingEpisodes(episodeIds: number[]) {
  const queue = useContext(QueueDetailsContext);

  return useMemo(() => {
    if (!queue) {
      return false;
    }

    return queue.some((item) =>
      item.episodeIds?.some((e) => episodeIds.includes(e))
    );
  }, [episodeIds, queue]);
}

export interface SeriesQueueDetails {
  count: number;
  episodesWithFiles: number;
}

export function useQueueDetailsForSeries(
  seriesId: number,
  seasonNumber?: number
) {
  const queue = useContext(QueueDetailsContext);

  return useMemo<SeriesQueueDetails>(() => {
    if (!queue) {
      return { count: 0, episodesWithFiles: 0 };
    }

    return queue.reduce<SeriesQueueDetails>(
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
  }, [seriesId, seasonNumber, queue]);
}

export const useQueueDetails = () => {
  return useContext(QueueDetailsContext) ?? [];
};
