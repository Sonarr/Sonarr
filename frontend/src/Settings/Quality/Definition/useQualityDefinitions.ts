import { useQueryClient } from '@tanstack/react-query';
import { useCallback, useMemo } from 'react';
import useApiMutation from 'Helpers/Hooks/useApiMutation';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import { usePendingItemsStore } from 'Helpers/Hooks/usePendingItemsStore';
import QualityDefinitionModel from 'Quality/QualityDefinitionModel';
import { useSaveSettings } from 'Settings/useSettings';

const PATH = '/qualitydefinition';
const DEFAULT_QUALITY_DEFINITIONS: QualityDefinitionModel[] = [];

export const useQualityDefinitions = () => {
  const result = useApiQuery<QualityDefinitionModel[]>({
    path: PATH,
  });

  return {
    ...result,
    data: result.data ?? DEFAULT_QUALITY_DEFINITIONS,
  };
};

export const useSaveQualityDefinitions = (onSuccess?: () => void) => {
  const queryClient = useQueryClient();

  const { mutate, isPending, error } = useApiMutation<
    QualityDefinitionModel[],
    QualityDefinitionModel[]
  >({
    path: PATH,
    method: 'PUT',
    mutationOptions: {
      onSuccess: (updatedSettings: QualityDefinitionModel[]) => {
        queryClient.setQueryData<QualityDefinitionModel[]>(
          [PATH],
          updatedSettings
        );
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

export const useManageQualityDefinitions = () => {
  const { data, isFetching, isFetched, error } = useQualityDefinitions();
  const {
    setPendingItem,
    clearPendingItems,
    getItemsWithPendingChanges,
    getPendingChangesForSave,
    hasPendingChanges,
  } = usePendingItemsStore<QualityDefinitionModel>();

  const { save, isSaving, saveError } = useSaveSettings(
    PATH,
    clearPendingItems
  );

  const settings = useMemo(() => {
    return {
      items: getItemsWithPendingChanges(data),
      hasPendingChanges,
    };
  }, [data, getItemsWithPendingChanges, hasPendingChanges]);

  const saveQualityDefinitions = useCallback(() => {
    const updatedSettings = getPendingChangesForSave(data);
    save(updatedSettings);
  }, [data, getPendingChangesForSave, save]);

  const updateDefinition = useCallback(
    <K extends keyof QualityDefinitionModel>(
      id: number,
      key: keyof QualityDefinitionModel,
      value: QualityDefinitionModel[K]
    ) => {
      const originalItem = data.find((def) => def.id === id);
      setPendingItem(id, key, value, originalItem);
    },
    [data, setPendingItem]
  );

  return {
    ...settings,
    updateDefinition,
    saveQualityDefinitions,
    isFetching,
    isFetched,
    isSaving,
    error,
    saveError,
  };
};
