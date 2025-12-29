import { useQueryClient } from '@tanstack/react-query';
import { useCallback, useMemo } from 'react';
import useApiMutation from 'Helpers/Hooks/useApiMutation';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import { usePendingChangesStore } from 'Helpers/Hooks/usePendingChangesStore';
import selectSettings from 'Store/Selectors/selectSettings';

export const useSettings = <T extends object>(path: string) => {
  const result = useApiQuery<T>({
    path,
  });

  return {
    ...result,
    data: result.data ?? ({} as T),
  };
};

export const useSaveSettings = <T extends object>(
  path: string,
  onSuccess?: () => void
) => {
  const queryClient = useQueryClient();

  const { mutate, isPending, error } = useApiMutation<T, T>({
    path,
    method: 'PUT',
    mutationOptions: {
      onSuccess: (updatedSettings: T) => {
        queryClient.setQueryData<T>([path], updatedSettings);
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

export const useManageSettings = <T extends object>(path: string) => {
  const { data, isFetching, isFetched, error } = useSettings<T>(path);
  const {
    pendingChanges,
    setPendingChange,
    unsetPendingChange,
    clearPendingChanges,
  } = usePendingChangesStore<T>({});

  const { save, isSaving, saveError } = useSaveSettings<T>(
    path,
    clearPendingChanges
  );

  const settings = useMemo(() => {
    return selectSettings<T>(data, pendingChanges, saveError);
  }, [data, pendingChanges, saveError]);

  const saveSettings = useCallback(() => {
    const updatedSettings = {
      ...data,
      ...pendingChanges,
    };

    save(updatedSettings);
  }, [data, pendingChanges, save]);

  const updateSetting = useCallback(
    <K extends keyof T>(key: K, value: T[K]) => {
      if (data[key] === value) {
        unsetPendingChange(key);
      } else {
        setPendingChange(key, value);
      }
    },
    [data, setPendingChange, unsetPendingChange]
  );

  return {
    ...settings,
    updateSetting,
    saveSettings,
    isFetching,
    isFetched,
    isSaving,
    error,
    saveError,
  };
};
