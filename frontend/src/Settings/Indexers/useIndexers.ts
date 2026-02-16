import { useQueryClient } from '@tanstack/react-query';
import { useMemo } from 'react';
import DownloadProtocol from 'DownloadClient/DownloadProtocol';
import useApiMutation from 'Helpers/Hooks/useApiMutation';
import {
  SelectedSchema,
  useProviderSchema,
  useSelectedSchema,
} from 'Settings/useProviderSchema';
import {
  useDeleteProvider,
  useManageProviderSettings,
  useProviderSettings,
} from 'Settings/useProviderSettings';
import Provider from 'typings/Provider';
import { sortByProp } from 'Utilities/Array/sortByProp';
import { ApiError } from 'Utilities/Fetch/fetchJson';
import translate from 'Utilities/String/translate';

export interface IndexerModel extends Provider {
  enableRss: boolean;
  enableAutomaticSearch: boolean;
  enableInteractiveSearch: boolean;
  supportsRss: boolean;
  supportsSearch: boolean;
  seasonSearchMaximumSingleEpisodeAge: number;
  protocol: DownloadProtocol;
  priority: number;
  downloadClientId: number;
  tags: number[];
}

interface BulkEditIndexersPayload {
  ids: number[];
  [key: string]: unknown;
}

interface BulkDeleteIndexersPayload {
  ids: number[];
}

const PATH = '/indexer';

export const useIndexersWithIds = (ids: number[]) => {
  const allIndexers = useIndexersData();

  return allIndexers.filter((indexer) => ids.includes(indexer.id));
};

export const useIndexer = (id: number | undefined) => {
  const { data } = useIndexers();

  if (id === undefined) {
    return undefined;
  }

  return data.find((indexer) => indexer.id === id);
};

export const useIndexersData = () => {
  const { data } = useIndexers();

  return data;
};

export const useSortedIndexers = () => {
  const result = useIndexers();

  const sortedData = useMemo(
    () => result.data.sort(sortByProp('name')),
    [result.data]
  );

  return {
    ...result,
    data: sortedData,
  };
};

export const useIndexers = () => {
  return useProviderSettings<IndexerModel>({
    path: PATH,
  });
};

export const useManageIndexer = (
  id: number | undefined,
  cloneId: number | undefined,
  selectedSchema?: SelectedSchema
) => {
  const schema = useSelectedSchema<IndexerModel>(PATH, selectedSchema);
  const cloneIndexer = useIndexer(cloneId);

  if (cloneId && !cloneIndexer) {
    throw new Error(`Indexer with ID ${cloneId} not found`);
  }

  if (selectedSchema && !schema) {
    throw new Error('A selected schema is required to manage metadata');
  }

  const defaultProvider = useMemo(() => {
    if (cloneId && cloneIndexer) {
      const clonedIndexer = {
        ...cloneIndexer,
        id: 0,
        name: translate('DefaultNameCopiedProfile', {
          name: cloneIndexer.name,
        }),
      };

      clonedIndexer.fields = clonedIndexer.fields.map((field) => {
        const newField = { ...field };

        if (newField.privacy === 'apiKey' || newField.privacy === 'password') {
          newField.value = '';
        }

        return newField;
      });

      return clonedIndexer;
    }

    if (selectedSchema && schema) {
      return {
        ...schema,
        name: schema.implementationName,
        enableRss: schema.supportsRss,
        enableAutomaticSearch: schema.supportsSearch,
        enableInteractiveSearch: schema.supportsSearch,
      };
    }

    return {} as IndexerModel;
  }, [cloneId, cloneIndexer, schema, selectedSchema]);

  const manage = useManageProviderSettings<IndexerModel>(
    id,
    defaultProvider,
    PATH
  );

  return manage;
};

export const useDeleteIndexer = (id: number) => {
  const result = useDeleteProvider<IndexerModel>(id, PATH);

  return {
    ...result,
    deleteIndexer: result.deleteProvider,
  };
};

export const useIndexerSchema = (enabled: boolean = true) => {
  return useProviderSchema<IndexerModel>(PATH, enabled);
};

export const useTestIndexer = (
  onSuccess?: () => void,
  onError?: (error: ApiError) => void
) => {
  const { mutate, isPending, error } = useApiMutation<void, IndexerModel>({
    path: `${PATH}/test`,
    method: 'POST',
    mutationOptions: {
      onSuccess,
      onError,
    },
  });

  return {
    testIndexer: mutate,
    isTesting: isPending,
    testError: error,
  };
};

export const useTestAllIndexers = (
  onSuccess?: () => void,
  onError?: (error: ApiError) => void
) => {
  const { mutate, isPending, error } = useApiMutation<void, void>({
    path: `${PATH}/testall`,
    method: 'POST',
    mutationOptions: {
      onSuccess,
      onError,
    },
  });

  return {
    testAllIndexers: mutate,
    isTestingAllIndexers: isPending,
    testAllError: error,
  };
};

export const useBulkEditIndexers = (
  onSuccess?: () => void,
  onError?: (error: ApiError) => void
) => {
  const queryClient = useQueryClient();

  const { mutate, isPending, error } = useApiMutation<
    IndexerModel[],
    BulkEditIndexersPayload
  >({
    path: `${PATH}/bulk`,
    method: 'PUT',
    mutationOptions: {
      onSuccess: (updatedIndexers) => {
        queryClient.setQueryData<IndexerModel[]>([PATH], (oldIndexers) => {
          if (!oldIndexers) {
            return oldIndexers;
          }

          return oldIndexers.map((indexer) => {
            const updatedIndexer = updatedIndexers.find(
              (updated) => updated.id === indexer.id
            );

            return updatedIndexer ? { ...indexer, ...updatedIndexer } : indexer;
          });
        });
        onSuccess?.();
      },
      onError,
    },
  });

  return {
    bulkEditIndexers: mutate,
    isSaving: isPending,
    bulkError: error,
  };
};

export const useBulkDeleteIndexers = (
  onSuccess?: () => void,
  onError?: (error: ApiError) => void
) => {
  const queryClient = useQueryClient();

  const { mutate, isPending, error } = useApiMutation<
    void,
    BulkDeleteIndexersPayload
  >({
    path: `${PATH}/bulk`,
    method: 'DELETE',
    mutationOptions: {
      onSuccess: (_, variables) => {
        const deletedIds = new Set(variables.ids);

        queryClient.setQueryData<IndexerModel[]>([PATH], (oldIndexers) => {
          if (!oldIndexers) {
            return oldIndexers;
          }

          return oldIndexers.filter((indexer) => !deletedIds.has(indexer.id));
        });
        onSuccess?.();
      },
      onError,
    },
  });

  return {
    bulkDeleteIndexers: mutate,
    isDeleting: isPending,
    bulkDeleteError: error,
  };
};
