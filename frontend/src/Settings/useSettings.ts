import { useQueryClient } from '@tanstack/react-query';
import { useCallback, useMemo } from 'react';
import useApiMutation from 'Helpers/Hooks/useApiMutation';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import { usePendingChangesStore } from 'Helpers/Hooks/usePendingChangesStore';
import selectSettings from 'Utilities/selectSettings';

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
  const { pendingChanges, setPendingChange, clearPendingChanges } =
    usePendingChangesStore<T>({});

  const { save, isSaving, saveError } = useSaveSettings<T>(
    path,
    clearPendingChanges
  );

  const changedValues = useMemo(() => {
    const changed: Partial<T> = {};

    (Object.keys(pendingChanges) as (keyof T)[]).forEach((key) => {
      if (data[key] !== pendingChanges[key]) {
        changed[key] = pendingChanges[key];
      }
    });

    return changed;
  }, [data, pendingChanges]);

  const settings = useMemo(() => {
    return selectSettings<T>(data, changedValues, saveError);
  }, [data, changedValues, saveError]);

  const saveSettings = useCallback(() => {
    const updatedSettings = {
      ...data,
      ...changedValues,
    };

    save(updatedSettings);
  }, [data, changedValues, save]);

  const updateSetting = useCallback(
    <K extends keyof T>(key: K, value: T[K]) => {
      setPendingChange(key, value);
    },
    [setPendingChange]
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
