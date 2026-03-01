import { useQueryClient } from '@tanstack/react-query';
import { useCallback, useMemo, useState } from 'react';
import ModelBase from 'App/ModelBase';
import useApiMutation from 'Helpers/Hooks/useApiMutation';
import useApiQuery, { QueryOptions } from 'Helpers/Hooks/useApiQuery';
import { usePendingChangesStore } from 'Helpers/Hooks/usePendingChangesStore';
import { usePendingFieldsStore } from 'Helpers/Hooks/usePendingFieldsStore';
import selectSettings from 'Store/Selectors/selectSettings';
import { PendingSection } from 'typings/pending';
import Provider from 'typings/Provider';
import { ApiError } from 'Utilities/Fetch/fetchJson';

interface ManageProviderSettings<T extends ModelBase>
  extends Omit<ReturnType<typeof selectSettings<T>>, 'settings'> {
  item: PendingSection<T>;
  updateValue: <K extends keyof T>(key: K, value: T[K]) => void;
  saveProvider: () => void;
  isSaving: boolean;
  saveError: ApiError | null;
  testProvider: () => void;
  isTesting: boolean;
  updateFieldValue?: (fieldProperties: Record<string, unknown>) => void;
}

const isProviderWithFields = (provider: unknown): provider is Provider => {
  return (
    typeof provider === 'object' &&
    provider !== null &&
    'fields' in provider &&
    Array.isArray((provider as Provider).fields)
  );
};

export const useProviderWithDefault = <T extends ModelBase>(
  id: number | undefined,
  defaultProvider: T,
  path: string
) => {
  const { data } = useProviderSettings<T>({ path });

  return useMemo(() => {
    if (!id) {
      return defaultProvider;
    }

    const provider = data.find((p) => p.id === id);

    if (!provider) {
      throw new Error(`Provider with ID ${id} not found`);
    }

    return provider;
  }, [data, defaultProvider, id]);
};

export const useProviderSettings = <T extends ModelBase>(
  options: QueryOptions<T[]>
) => {
  const result = useApiQuery<T[]>(options);

  return {
    ...result,
    data: result.data ?? ([] as T[]),
  };
};

export const useSaveProviderSettings = <T extends ModelBase>(
  id: number,
  path: string,
  onSuccess?: (updatedSettings: T) => void,
  onError?: (error: ApiError) => void
) => {
  const queryClient = useQueryClient();

  const { mutate, isPending, error } = useApiMutation<T, T>({
    path: id ? `${path}/${id}` : path,
    method: id ? 'PUT' : 'POST',
    mutationOptions: {
      onSuccess: (updatedSettings: T) => {
        queryClient.setQueryData<T[]>([path], (oldData = []) => {
          if (id) {
            return oldData.map((item) =>
              item.id === updatedSettings.id ? updatedSettings : item
            );
          }

          return [...oldData, updatedSettings];
        });
        onSuccess?.(updatedSettings);
      },
      onError,
    },
  });

  return {
    save: mutate,
    isSaving: isPending,
    saveError: error,
  };
};

export const useTestProvider = <T extends ModelBase>(
  path: string,
  onSuccess?: () => void,
  onError?: (error: ApiError) => void
) => {
  const { mutate, isPending, error } = useApiMutation<void, T>({
    path: `${path}/test`,
    method: 'POST',
    mutationOptions: {
      onSuccess,
      onError,
    },
  });

  return {
    test: mutate,
    isTesting: isPending,
    testError: error,
  };
};

export const useManageProviderSettings = <T extends ModelBase>(
  id: number | undefined,
  defaultProvider: T,
  path: string
): ManageProviderSettings<T> => {
  const provider = useProviderWithDefault<T>(id, defaultProvider, path);
  const [mutationError, setMutationError] = useState<ApiError | null>(null);

  const {
    pendingChanges,
    setPendingChange,
    unsetPendingChange,
    clearPendingChanges,
  } = usePendingChangesStore<T>({});

  const {
    pendingFields,
    setPendingFields,
    clearPendingFields,
    hasPendingFields,
  } = usePendingFieldsStore();

  const handleSaveSuccess = useCallback(() => {
    setMutationError(null);
    clearPendingChanges();
    clearPendingFields();
  }, [clearPendingChanges, clearPendingFields]);

  const handleTestSuccess = useCallback(() => {
    setMutationError(null);
  }, []);

  const { save, isSaving } = useSaveProviderSettings<T>(
    provider.id,
    path,
    handleSaveSuccess,
    setMutationError
  );

  const { test, isTesting } = useTestProvider<T>(
    path,
    handleTestSuccess,
    setMutationError
  );

  const { settings: item, ...settings } = useMemo(() => {
    // Create a combined pending changes object that includes fields
    const combinedPendingChanges = hasPendingFields
      ? {
          ...pendingChanges,
          fields: Object.fromEntries(pendingFields),
        }
      : pendingChanges;

    return selectSettings<T>(provider, combinedPendingChanges, mutationError);
  }, [
    provider,
    pendingChanges,
    pendingFields,
    hasPendingFields,
    mutationError,
  ]);

  const saveProvider = useCallback(() => {
    let updatedSettings: T = {
      ...provider,
      ...pendingChanges,
    };

    // If there are pending field changes and the provider has fields
    if (isProviderWithFields(provider)) {
      const fields = provider.fields.map((field) => {
        if (pendingFields.has(field.name)) {
          return {
            name: field.name,
            value: pendingFields.get(field.name),
          };
        }

        return {
          name: field.name,
          value: field.value,
        };
      });

      updatedSettings = {
        ...updatedSettings,
        fields,
      } as T;
    }

    save(updatedSettings);
  }, [provider, pendingChanges, pendingFields, save]);

  const testProvider = useCallback(() => {
    let updatedSettings: T = {
      ...provider,
      ...pendingChanges,
    };

    // If there are pending field changes and the provider has fields
    if (isProviderWithFields(provider)) {
      const fields = provider.fields.map((field) => {
        if (pendingFields.has(field.name)) {
          return {
            ...field,
            value: pendingFields.get(field.name),
          };
        }
        return field;
      });

      updatedSettings = {
        ...updatedSettings,
        fields,
      } as T;
    }

    test(updatedSettings);
  }, [provider, pendingChanges, pendingFields, test]);

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

  const hasFields = useMemo(() => {
    return 'fields' in provider && Array.isArray(provider.fields);
  }, [provider]);

  const updateFieldValue = useCallback(
    (fieldProperties: Record<string, unknown>) => {
      if (!isProviderWithFields(provider)) {
        throw new Error('updateFieldValue called on provider without fields');
      }

      const providerFields = provider.fields;
      const currentFields = pendingFields;
      const newFields = { ...currentFields, ...fieldProperties };

      // Check if the new fields are different from the provider's current fields
      const hasChanges = Object.entries(newFields).some(([key, value]) => {
        const currentField = providerFields.find((f) => f.name === key);
        return currentField?.value !== value;
      });

      if (hasChanges) {
        setPendingFields(newFields);
      } else {
        clearPendingFields();
      }
    },
    [pendingFields, provider, setPendingFields, clearPendingFields]
  );

  const baseReturn = {
    ...settings,
    item,
    updateValue,
    saveProvider,
    isSaving,
    saveError: mutationError,
    testProvider,
    isTesting,
  };

  if (hasFields) {
    return {
      ...baseReturn,
      updateFieldValue,
    };
  }

  return baseReturn;
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
