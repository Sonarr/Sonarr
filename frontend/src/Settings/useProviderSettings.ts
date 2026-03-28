import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useCallback, useMemo, useRef, useState } from 'react';
import ModelBase from 'App/ModelBase';
import useApiMutation, {
  getValidationFailures,
} from 'Helpers/Hooks/useApiMutation';
import useApiQuery, { QueryOptions } from 'Helpers/Hooks/useApiQuery';
import { usePendingChangesStore } from 'Helpers/Hooks/usePendingChangesStore';
import { usePendingFieldsStore } from 'Helpers/Hooks/usePendingFieldsStore';
import selectSettings from 'Store/Selectors/selectSettings';
import { PendingSection } from 'typings/pending';
import Provider from 'typings/Provider';
import fetchJson, { ApiError } from 'Utilities/Fetch/fetchJson';
import getQueryPath from 'Utilities/Fetch/getQueryPath';
import getQueryString, { QueryParams } from 'Utilities/Fetch/getQueryString';

export type SkipValidation = 'none' | 'warnings' | 'all';
export interface SaveOptions {
  skipTesting?: boolean;
  skipValidation?: SkipValidation;
}

interface BaseManageProviderSettings<T extends ModelBase>
  extends Omit<ReturnType<typeof selectSettings<T>>, 'settings'> {
  item: PendingSection<T>;
  updateValue: <K extends keyof T>(key: K, value: T[K]) => void;
  saveProvider: () => void;
  isSaving: boolean;
  saveError: ApiError | null;
  testProvider: () => void;
  isTesting: boolean;
}

interface ManageProviderSettingsWithFields<T extends ModelBase>
  extends BaseManageProviderSettings<T> {
  updateFieldValue: (fieldProperties: Record<string, unknown>) => void;
}

type ManageProviderSettings<T extends ModelBase> = T extends Provider
  ? ManageProviderSettingsWithFields<T>
  : BaseManageProviderSettings<T>;

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

  const { mutate, isPending, error } = useMutation<
    T,
    ApiError,
    {
      data: T;
    } & SaveOptions
  >({
    mutationFn: async ({ data, skipTesting, skipValidation }) => {
      const queryParams: QueryParams = {};

      if (skipTesting) {
        queryParams.skipTesting = true;
      }

      if (skipValidation && skipValidation !== 'none') {
        queryParams.skipValidation = skipValidation;
      }

      return fetchJson<T, T>({
        path:
          getQueryPath(id ? `${path}/${id}` : path) +
          getQueryString(queryParams),
        method: id ? 'PUT' : 'POST',
        headers: {
          'X-Api-Key': window.Sonarr.apiKey,
          'X-Sonarr-Client': 'Sonarr',
        },
        body: data,
      });
    },
    onSuccess: (updatedSettings: T) => {
      queryClient.setQueryData<T[]>([path], (oldData = []) => {
        const existingIndex = oldData.findIndex(
          (item) => item.id === updatedSettings.id
        );

        if (existingIndex === -1) {
          return [...oldData, updatedSettings];
        }

        return oldData.map((item) =>
          item.id === updatedSettings.id ? updatedSettings : item
        );
      });
      onSuccess?.(updatedSettings);
    },
    onError,
  });

  const save = useCallback(
    (data: T, options?: SaveOptions) => {
      mutate({ data, ...options });
    },
    [mutate]
  );

  return {
    save,
    isSaving: isPending,
    saveError: error,
  };
};

export const useTestProvider = <T extends ModelBase>(
  path: string,
  onSuccess?: () => void,
  onError?: (error: ApiError) => void
) => {
  const { mutate, isPending, error } = useMutation<
    void,
    ApiError,
    { data: T } & SaveOptions
  >({
    mutationFn: async ({ data, skipValidation }) => {
      const queryParams: QueryParams = {};

      if (skipValidation && skipValidation !== 'none') {
        queryParams.skipValidation = skipValidation;
      }

      return fetchJson<void, T>({
        path: getQueryPath(`${path}/test`) + getQueryString(queryParams),
        method: 'POST',
        headers: {
          'X-Api-Key': window.Sonarr.apiKey,
          'X-Sonarr-Client': 'Sonarr',
        },
        body: data,
      });
    },
    onSuccess,
    onError,
  });

  const test = useCallback(
    (data: T, options?: SaveOptions) => {
      mutate({ data, ...options });
    },
    [mutate]
  );

  return {
    test,
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
  const lastSaveData = useRef<string | null>(null);

  const {
    pendingChanges,
    setPendingChange,
    unsetPendingChange,
    clearPendingChanges,
    hasPendingChanges,
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
    lastSaveData.current = null;
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

    const serializedSettings = JSON.stringify(updatedSettings);
    const isResave = lastSaveData.current === serializedSettings;
    lastSaveData.current = serializedSettings;

    const saveOptions: SaveOptions = {};

    // For existing providers with no pending changes, skip testing and all validation.
    if (provider.id > 0 && !hasPendingChanges && !hasPendingFields) {
      saveOptions.skipTesting = true;
      saveOptions.skipValidation = 'all';
    } else {
      // If resaving the exact same settings as the previous attempt, skip testing.
      if (isResave) {
        saveOptions.skipTesting = true;
      }

      // If the last save returned only warnings, skip warning validation on the next save.
      const { errors, warnings } = getValidationFailures(mutationError);

      if (errors.length === 0 && warnings.length > 0) {
        saveOptions.skipValidation = 'warnings';
      }
    }

    save(updatedSettings, saveOptions);
  }, [
    provider,
    pendingChanges,
    pendingFields,
    hasPendingChanges,
    hasPendingFields,
    mutationError,
    save,
  ]);

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

    const testOptions: SaveOptions = {};

    // If the last operation returned only warnings, skip warning validation on the next test.
    const { errors, warnings } = getValidationFailures(mutationError);

    if (errors.length === 0 && warnings.length > 0) {
      testOptions.skipValidation = 'warnings';
    }

    test(updatedSettings, testOptions);
  }, [provider, pendingChanges, pendingFields, mutationError, test]);

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
    } as ManageProviderSettings<T>;
  }

  return baseReturn as ManageProviderSettings<T>;
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
