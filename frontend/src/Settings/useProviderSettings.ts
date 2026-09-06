import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useCallback, useMemo, useRef } from 'react';
import ModelBase from 'App/ModelBase';
import useApiMutation, {
  addOrUpdateQueryClientItem,
  getValidationFailures,
} from 'Helpers/Hooks/useApiMutation';
import useApiQuery, { QueryOptions } from 'Helpers/Hooks/useApiQuery';
import { usePendingChangesStore } from 'Helpers/Hooks/usePendingChangesStore';
import { usePendingFieldsStore } from 'Helpers/Hooks/usePendingFieldsStore';
import { PendingSection } from 'typings/pending';
import Provider from 'typings/Provider';
import fetchJson, { ApiError } from 'Utilities/Fetch/fetchJson';
import getQueryPath from 'Utilities/Fetch/getQueryPath';
import getQueryString, { QueryParams } from 'Utilities/Fetch/getQueryString';
import selectSettings from 'Utilities/selectSettings';

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
  onSuccess?: (updatedSettings: T) => void
) => {
  const queryClient = useQueryClient();

  const { mutate, isPending, error, submittedAt } = useMutation<
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
      queryClient.setQueryData<T[]>([path], (oldData = []) =>
        addOrUpdateQueryClientItem(oldData, updatedSettings, 'id')
      );
      onSuccess?.(updatedSettings);
    },
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
    saveSubmittedAt: submittedAt,
  };
};

export const useTestProvider = <T extends ModelBase>(path: string) => {
  const { mutate, isPending, error, submittedAt } = useMutation<
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
    testSubmittedAt: submittedAt,
  };
};

export const useManageProviderSettings = <T extends ModelBase>(
  id: number | undefined,
  defaultProvider: T,
  path: string
): ManageProviderSettings<T> => {
  const provider = useProviderWithDefault<T>(id, defaultProvider, path);
  const lastSaveData = useRef<string | null>(null);

  const { pendingChanges, setPendingChange, clearPendingChanges } =
    usePendingChangesStore<T>({});

  const { pendingFields, setPendingFields, clearPendingFields } =
    usePendingFieldsStore();

  const handleSaveSuccess = useCallback(() => {
    clearPendingChanges();
    clearPendingFields();
    lastSaveData.current = null;
  }, [clearPendingChanges, clearPendingFields]);

  const { save, isSaving, saveError, saveSubmittedAt } =
    useSaveProviderSettings<T>(provider.id, path, handleSaveSuccess);

  const { test, isTesting, testError, testSubmittedAt } =
    useTestProvider<T>(path);

  const mutationError =
    saveSubmittedAt >= testSubmittedAt ? saveError : testError;

  const changedValues = useMemo(() => {
    const changed: Partial<T> = {};

    (Object.keys(pendingChanges) as (keyof T)[]).forEach((key) => {
      if (provider[key] !== pendingChanges[key]) {
        changed[key] = pendingChanges[key];
      }
    });

    return changed;
  }, [provider, pendingChanges]);

  const changedFields = useMemo(() => {
    const changed = new Map<string, unknown>();

    if (!isProviderWithFields(provider)) {
      return changed;
    }

    pendingFields.forEach((value, name) => {
      if (provider.fields.find((f) => f.name === name)?.value !== value) {
        changed.set(name, value);
      }
    });

    return changed;
  }, [provider, pendingFields]);

  const hasChangedValues = Object.keys(changedValues).length > 0;
  const hasChangedFields = changedFields.size > 0;

  const { settings: item, ...settings } = useMemo(() => {
    // Create a combined pending changes object that includes fields
    const combinedPendingChanges = hasChangedFields
      ? {
          ...changedValues,
          fields: Object.fromEntries(changedFields),
        }
      : changedValues;

    return selectSettings<T>(provider, combinedPendingChanges, mutationError);
  }, [provider, changedValues, changedFields, hasChangedFields, mutationError]);

  const saveProvider = useCallback(() => {
    let updatedSettings: T = {
      ...provider,
      ...changedValues,
    };

    // If there are pending field changes and the provider has fields
    if (isProviderWithFields(provider)) {
      const fields = provider.fields.map((field) => {
        if (changedFields.has(field.name)) {
          return {
            name: field.name,
            value: changedFields.get(field.name),
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
    if (provider.id > 0 && !hasChangedValues && !hasChangedFields) {
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
    changedValues,
    changedFields,
    hasChangedValues,
    hasChangedFields,
    mutationError,
    save,
  ]);

  const testProvider = useCallback(() => {
    let updatedSettings: T = {
      ...provider,
      ...changedValues,
    };

    // If there are pending field changes and the provider has fields
    if (isProviderWithFields(provider)) {
      const fields = provider.fields.map((field) => {
        if (changedFields.has(field.name)) {
          return {
            ...field,
            value: changedFields.get(field.name),
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
  }, [provider, changedValues, changedFields, mutationError, test]);

  const updateValue = useCallback(
    <K extends keyof T>(key: K, value: T[K]) => {
      setPendingChange(key, value);
    },
    [setPendingChange]
  );

  const hasFields = useMemo(() => {
    return 'fields' in provider && Array.isArray(provider.fields);
  }, [provider]);

  const updateFieldValue = useCallback(
    (fieldProperties: Record<string, unknown>) => {
      setPendingFields(fieldProperties);
    },
    [setPendingFields]
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
