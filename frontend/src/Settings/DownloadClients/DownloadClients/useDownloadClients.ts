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

export interface DownloadClientModel extends Provider {
  enable: boolean;
  protocol: DownloadProtocol;
  priority: number;
  removeCompletedDownloads: boolean;
  removeFailedDownloads: boolean;
  tags: number[];
}

interface BulkEditDownloadClientsPayload {
  ids: number[];
  [key: string]: unknown;
}

interface BulkDeleteDownloadClientsPayload {
  ids: number[];
}

const PATH = '/downloadclient';

export const useDownloadClients = () => {
  return useProviderSettings<DownloadClientModel>({
    path: PATH,
  });
};

export const useDownloadClientsData = () => {
  const { data } = useDownloadClients();

  return data;
};

export const useSortedDownloadClients = () => {
  const result = useDownloadClients();

  const sortedData = useMemo(
    () => [...result.data].sort(sortByProp('name')),
    [result.data]
  );

  return {
    ...result,
    data: sortedData,
  };
};

export const useDownloadClientsWithIds = (ids: number[]) => {
  const data = useDownloadClientsData();

  return data.filter((downloadClient) => ids.includes(downloadClient.id));
};

export const useDownloadClient = (id: number | undefined) => {
  const { data } = useDownloadClients();

  if (id === undefined) {
    return undefined;
  }

  return data.find((downloadClient) => downloadClient.id === id);
};

export const useEnabledDownloadClients = (protocol: DownloadProtocol) => {
  const result = useDownloadClients();

  const enabled = useMemo(
    () =>
      [...result.data]
        .filter((item) => item.enable && item.protocol === protocol)
        .sort(sortByProp('name')),
    [result.data, protocol]
  );

  return { ...result, data: enabled };
};

export const useManageDownloadClient = (
  id: number | undefined,
  cloneId: number | undefined,
  selectedSchema?: SelectedSchema
) => {
  const schema = useSelectedSchema<DownloadClientModel>(PATH, selectedSchema);
  const cloneDownloadClient = useDownloadClient(cloneId);

  if (cloneId && !cloneDownloadClient) {
    throw new Error(`Download client with ID ${cloneId} not found`);
  }

  if (selectedSchema && !schema) {
    throw new Error('A selected schema is required to manage metadata');
  }

  const defaultProvider = useMemo(() => {
    if (cloneId && cloneDownloadClient) {
      const cloned = {
        ...cloneDownloadClient,
        id: 0,
        name: translate('DefaultNameCopiedProfile', {
          name: cloneDownloadClient.name,
        }),
      };

      cloned.fields = cloned.fields.map((field) => {
        const newField = { ...field };

        if (newField.privacy === 'apiKey' || newField.privacy === 'password') {
          newField.value = '';
        }

        return newField;
      });

      return cloned;
    }

    if (selectedSchema && schema) {
      return {
        ...schema,
        name: schema.implementationName,
        enable: true,
      };
    }

    return {} as DownloadClientModel;
  }, [cloneId, cloneDownloadClient, schema, selectedSchema]);

  return useManageProviderSettings<DownloadClientModel>(
    id,
    defaultProvider,
    PATH
  );
};

export const useDeleteDownloadClient = (id: number) => {
  const result = useDeleteProvider<DownloadClientModel>(id, PATH);

  return {
    ...result,
    deleteDownloadClient: result.deleteProvider,
  };
};

export const useDownloadClientSchema = (enabled: boolean = true) => {
  return useProviderSchema<DownloadClientModel>(PATH, enabled);
};

export const useTestDownloadClient = (
  onSuccess?: () => void,
  onError?: (error: ApiError) => void
) => {
  const { mutate, isPending, error } = useApiMutation<
    void,
    DownloadClientModel
  >({
    path: `${PATH}/test`,
    method: 'POST',
    mutationOptions: {
      onSuccess,
      onError,
    },
  });

  return {
    testDownloadClient: mutate,
    isTesting: isPending,
    testError: error,
  };
};

export const useTestAllDownloadClients = (
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
    testAllDownloadClients: mutate,
    isTestingAllDownloadClients: isPending,
    testAllError: error,
  };
};

export const useBulkEditDownloadClients = (
  onSuccess?: () => void,
  onError?: (error: ApiError) => void
) => {
  const queryClient = useQueryClient();

  const { mutate, isPending, error } = useApiMutation<
    DownloadClientModel[],
    BulkEditDownloadClientsPayload
  >({
    path: `${PATH}/bulk`,
    method: 'PUT',
    mutationOptions: {
      onSuccess: (updated) => {
        queryClient.setQueryData<DownloadClientModel[]>([PATH], (oldData) => {
          if (!oldData) {
            return oldData;
          }

          return oldData.map((downloadClient) => {
            const updatedClient = updated.find(
              (u) => u.id === downloadClient.id
            );

            return updatedClient
              ? { ...downloadClient, ...updatedClient }
              : downloadClient;
          });
        });
        onSuccess?.();
      },
      onError,
    },
  });

  return {
    bulkEditDownloadClients: mutate,
    isSaving: isPending,
    bulkError: error,
  };
};

export const useBulkDeleteDownloadClients = (
  onSuccess?: () => void,
  onError?: (error: ApiError) => void
) => {
  const queryClient = useQueryClient();

  const { mutate, isPending, error } = useApiMutation<
    void,
    BulkDeleteDownloadClientsPayload
  >({
    path: `${PATH}/bulk`,
    method: 'DELETE',
    mutationOptions: {
      onSuccess: (_, variables) => {
        const deletedIds = new Set(variables.ids);

        queryClient.setQueryData<DownloadClientModel[]>([PATH], (oldData) => {
          if (!oldData) {
            return oldData;
          }

          return oldData.filter(
            (downloadClient) => !deletedIds.has(downloadClient.id)
          );
        });
        onSuccess?.();
      },
      onError,
    },
  });

  return {
    bulkDeleteDownloadClients: mutate,
    isDeleting: isPending,
    bulkDeleteError: error,
  };
};
