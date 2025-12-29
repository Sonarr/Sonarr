import { useQueryClient } from '@tanstack/react-query';
import { useCallback, useMemo } from 'react';
import ModelBase from 'App/ModelBase';
import useApiMutation from 'Helpers/Hooks/useApiMutation';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import { usePendingChangesStore } from 'Helpers/Hooks/usePendingChangesStore';
import selectSettings from 'Store/Selectors/selectSettings';

export const useProvider = <T extends ModelBase>(
  id: number,
  defaultProvider: T,
  path: string
) => {
  const { data } = useProviderSettings<T>(path);

  return useMemo(() => {
    if (id === 0) {
      return defaultProvider;
    }

    const provider = data.find((p) => p.id === id);

    if (!provider) {
      throw new Error(`Provider with ID ${id} not found`);
    }

    return provider;
  }, [data, defaultProvider, id]);
};

export const useProviderSettings = <T extends ModelBase>(path: string) => {
  const result = useApiQuery<T[]>({
    path,
  });

  return {
    ...result,
    data: result.data ?? ([] as T[]),
  };
};

export const useSaveProviderSettings = <T extends ModelBase>(
  id: number,
  path: string,
  onSuccess?: () => void
) => {
  const queryClient = useQueryClient();

  const { mutate, isPending, error } = useApiMutation<T, T>({
    path: id === 0 ? path : `${path}/${id}`,
    method: id === 0 ? 'POST' : 'PUT',
    mutationOptions: {
      onSuccess: (updatedSettings: T) => {
        queryClient.setQueryData<T[]>([path], (oldData = []) => {
          if (id === 0) {
            return [...oldData, updatedSettings];
          }

          return oldData.map((item) =>
            item.id === updatedSettings.id ? updatedSettings : item
          );
        });
        onSuccess?.();
      },
    },
  });

  return {
    save: mutate,
    isSaving: isPending,
    saveError: error,
  };
};

export const useManageProviderSettings = <T extends ModelBase>(
  id: number,
  defaultProvider: T,
  path: string
) => {
  const provider = useProvider<T>(id, defaultProvider, path);

  const {
    pendingChanges,
    setPendingChange,
    unsetPendingChange,
    clearPendingChanges,
  } = usePendingChangesStore<T>({});

  const { save, isSaving, saveError } = useSaveProviderSettings<T>(
    provider.id,
    path,
    clearPendingChanges
  );

  const { settings: item, ...settings } = useMemo(() => {
    return selectSettings<T>(provider, pendingChanges, saveError);
  }, [provider, pendingChanges, saveError]);

  const saveProvider = useCallback(() => {
    const updatedSettings = {
      ...provider,
      ...pendingChanges,
    };

    save(updatedSettings);
  }, [provider, pendingChanges, save]);

  const updateValue = useCallback(
    <K extends keyof T>(key: K, value: T[K]) => {
      if (provider[key] === value) {
        unsetPendingChange(key);
      } else {
        setPendingChange(key, value);
      }
    },
    [provider, setPendingChange, unsetPendingChange]
  );

  return {
    ...settings,
    item,
    updateValue,
    saveProvider,
    isSaving,
    saveError,
  };
};

export const useDeleteProvider = <T extends ModelBase>(
  id: number,
  path: string
) => {
  const queryClient = useQueryClient();

  const { mutate, isPending, error } = useApiMutation<void, void>({
    path: `${path}/${id}`,
    method: 'DELETE',
    mutationOptions: {
      onSuccess: () => {
        queryClient.setQueryData<T[]>([path], (oldData = []) => {
          return oldData.filter((item) => item.id !== id);
        });
      },
    },
  });

  return {
    deleteProvider: mutate,
    isDeleting: isPending,
    deleteError: error,
  };
};
