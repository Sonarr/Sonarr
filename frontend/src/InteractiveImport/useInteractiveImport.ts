import { useQueryClient } from '@tanstack/react-query';
import { useCallback, useMemo } from 'react';
import ModelBase from 'App/ModelBase';
import useApiMutation from 'Helpers/Hooks/useApiMutation';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import Language from 'Language/Language';
import { QualityModel } from 'Quality/Quality';
import clientSideFilterAndSort from 'Utilities/Filter/clientSideFilterAndSort';
import InteractiveImport from './InteractiveImport';
import { useInteractiveImportOptions } from './interactiveImportOptionsStore';
import ReleaseType from './ReleaseType';

const DEFAULT_ITEMS: InteractiveImport[] = [];

interface InteractiveImportParams {
  downloadId?: string;
  seriesId?: number;
  seasonNumber?: number;
  folder?: string;
  filterExistingFiles?: boolean;
}

const useInteractiveImport = (params: InteractiveImportParams) => {
  const { sortKey, sortDirection } = useInteractiveImportOptions();

  const { data, isFetching, isFetched, error, refetch } = useApiQuery<
    InteractiveImport[]
  >({
    path: '/manualimport',
    queryParams: { ...params },
    queryOptions: {
      // Set to 0 so we don't persist the data after the modal is closed and the query becomes inactive
      gcTime: 0,
      // Disable refetch on window focus to prevent refetching when the user switch tabs
      refetchOnWindowFocus: false,
    },
  });

  const items = data ?? DEFAULT_ITEMS;
  const originalItems = [...items];

  const { data: sortedItems } = useMemo(() => {
    const sortPredicates = {
      series: (item: InteractiveImport) => item.series?.title || '',
      quality: (item: InteractiveImport) => item.quality?.quality?.name || '',
      languages: (item: InteractiveImport) =>
        item.languages?.map((l) => l.name).join(', ') || '',
    };

    return clientSideFilterAndSort(items, {
      sortKey,
      sortDirection,
      sortPredicates,
    });
  }, [items, sortKey, sortDirection]);

  return {
    data: sortedItems,
    originalItems,
    isFetching,
    isFetched,
    error,
    refetch,
  };
};

export default useInteractiveImport;

export const useUpdateInteractiveImportItem = () => {
  const queryClient = useQueryClient();

  const updateInteractiveImportItem = useCallback(
    (id: number, updates: Partial<InteractiveImport>) => {
      queryClient.setQueriesData(
        { queryKey: ['/manualimport'] },
        (oldData: InteractiveImport[] | undefined) => {
          if (!oldData) {
            return oldData;
          }

          return oldData.map((item) => {
            return item.id === id ? { ...item, ...updates } : item;
          });
        }
      );
    },
    [queryClient]
  );

  return { updateInteractiveImportItem };
};

export const useUpdateInteractiveImportItems = () => {
  const queryClient = useQueryClient();

  const updateInteractiveImportItems = useCallback(
    (ids: number[], updates: Partial<InteractiveImport>) => {
      queryClient.setQueriesData(
        { queryKey: ['/manualimport'] },
        (oldData: InteractiveImport[] | undefined) => {
          if (!oldData) {
            return oldData;
          }

          return oldData.map((item) => {
            return ids.includes(item.id) ? { ...item, ...updates } : item;
          });
        }
      );
    },
    [queryClient]
  );

  return { updateInteractiveImportItems };
};

interface ReprocessInteractiveImportItem extends ModelBase {
  path: string;
  seriesId: number | undefined;
  seasonNumber: number | undefined;
  episodeIds: number[] | undefined;
  quality: QualityModel | undefined;
  languages: Language[];
  releaseGroup: string | undefined;
  downloadId: string | undefined;
  indexerFlags: number;
  releaseType: ReleaseType;
}

export const useReprocessInteractiveImportItems = () => {
  const queryClient = useQueryClient();

  const { mutate, isPending, error } = useApiMutation<
    InteractiveImport[],
    ReprocessInteractiveImportItem[]
  >({
    path: '/manualimport',
    method: 'POST',
    mutationOptions: {
      onSuccess: (updatedItems) => {
        queryClient.setQueriesData(
          { queryKey: ['/manualimport'] },
          (oldData: InteractiveImport[] | undefined) => {
            if (!oldData) {
              return oldData;
            }

            return oldData.map((oldItem: InteractiveImport) => {
              const reprocessedItem = updatedItems.find(
                (updatedItem) => updatedItem.id === oldItem.id
              );

              return reprocessedItem ? reprocessedItem : oldItem;
            });
          }
        );
      },
    },
  });

  const reprocessInteractiveImportItems = useCallback(
    (ids: number[]) => {
      const [, currentData] = queryClient.getQueriesData<InteractiveImport[]>({
        queryKey: ['/manualimport'],
      })[0];

      if (!currentData) {
        console.info('\x1b[36m[MarkTest] no data\x1b[0m');
        return;
      }

      const requestPayload = ids.reduce<ReprocessInteractiveImportItem[]>(
        (acc, id) => {
          const item = currentData.find((i) => i.id === id);

          if (!item) {
            return acc;
          }

          acc.push({
            id,
            path: item.path,
            seriesId: item.series ? item.series.id : undefined,
            seasonNumber: item.seasonNumber,
            episodeIds: (item.episodes || []).map((e) => e.id),
            quality: item.quality,
            languages: item.languages,
            releaseGroup: item.releaseGroup,
            indexerFlags: item.indexerFlags,
            releaseType: item.releaseType,
            downloadId: item.downloadId,
          });

          return acc;
        },
        []
      );

      mutate(requestPayload);
    },
    [queryClient, mutate]
  );

  return {
    reprocessInteractiveImportItems,
    isReprocessing: isPending,
    error,
  };
};
