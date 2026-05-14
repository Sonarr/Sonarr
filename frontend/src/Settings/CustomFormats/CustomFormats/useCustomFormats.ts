import { useQueryClient } from '@tanstack/react-query';
import { useCallback, useMemo } from 'react';
import ModelBase from 'App/ModelBase';
import useApiMutation from 'Helpers/Hooks/useApiMutation';
import { useProviderSchema } from 'Settings/useProviderSchema';
import {
  useDeleteProvider,
  useManageProviderSettings,
  useProviderSettings,
} from 'Settings/useProviderSettings';
import Field from 'typings/Field';
import { sortByProp } from 'Utilities/Array/sortByProp';
import { ApiError } from 'Utilities/Fetch/fetchJson';
import translate from 'Utilities/String/translate';

export interface CustomFormatSpecification {
  id: number;
  name: string;
  implementation: string;
  implementationName: string;
  infoLink: string;
  negate: boolean;
  required: boolean;
  fields: Field[];
}

export interface CustomFormat extends ModelBase {
  name: string;
  includeCustomFormatWhenRenaming: boolean;
  specifications: CustomFormatSpecification[];
}

interface BulkEditCustomFormatsPayload {
  ids: number[];
  includeCustomFormatWhenRenaming?: boolean;
}

interface BulkDeleteCustomFormatsPayload {
  ids: number[];
}

const PATH = '/customformat';

const DEFAULT_CUSTOM_FORMAT: CustomFormat = {
  id: 0,
  name: '',
  includeCustomFormatWhenRenaming: false,
  specifications: [],
};

export const useCustomFormats = () => {
  return useProviderSettings<CustomFormat>({
    path: PATH,
    queryOptions: {
      refetchOnWindowFocus: false,
    },
  });
};

export const useSortedCustomFormats = () => {
  const result = useCustomFormats();

  const sortedData = useMemo(
    () => [...result.data].sort(sortByProp('name')),
    [result.data]
  );

  return { ...result, data: sortedData };
};

export const useCustomFormat = (id: number | undefined) => {
  const { data } = useCustomFormats();

  if (id === undefined) {
    return undefined;
  }

  return data.find((cf) => cf.id === id);
};

export const useCustomFormatsWithIds = (ids: number[]) => {
  const { data } = useCustomFormats();

  return data.filter((cf) => ids.includes(cf.id));
};

export const useDeleteCustomFormat = (id: number) => {
  const result = useDeleteProvider<CustomFormat>(id, PATH);

  return {
    ...result,
    deleteCustomFormat: result.deleteProvider,
  };
};

export const useCustomFormatSchema = (enabled: boolean = true) => {
  return useProviderSchema<CustomFormatSpecification>(PATH, enabled);
};

function getNextSpecId(specifications: CustomFormatSpecification[]) {
  return specifications.length > 0
    ? Math.max(...specifications.map((s) => s.id)) + 1
    : 1;
}

export const useManageCustomFormat = (
  id: number | undefined,
  cloneId: number | undefined
) => {
  const cloneSource = useCustomFormat(cloneId);

  if (cloneId && !cloneSource) {
    throw new Error(`CustomFormat with ID ${cloneId} not found`);
  }

  const defaultProvider = useMemo<CustomFormat>(() => {
    if (cloneId && cloneSource) {
      return {
        ...cloneSource,
        id: 0,
        name: translate('DefaultNameCopiedProfile', {
          name: cloneSource.name,
        }),
      };
    }

    return DEFAULT_CUSTOM_FORMAT;
  }, [cloneId, cloneSource]);

  const manage = useManageProviderSettings<CustomFormat>(
    id,
    defaultProvider,
    PATH
  );

  const specifications = useMemo(
    () =>
      manage.item.specifications.value.map((spec, i) => ({
        ...spec,
        id: spec.id ?? i + 1,
      })),
    [manage.item.specifications.value]
  );

  const saveSpecification = useCallback(
    (spec: CustomFormatSpecification) => {
      if (spec.id > 0 && specifications.some((s) => s.id === spec.id)) {
        manage.updateValue(
          'specifications',
          specifications.map((s) => (s.id === spec.id ? spec : s))
        );
        return;
      }

      const newId = getNextSpecId(specifications);

      manage.updateValue('specifications', [
        ...specifications,
        { ...spec, id: newId },
      ]);
    },
    [specifications, manage]
  );

  const deleteSpecification = useCallback(
    (specId: number) => {
      manage.updateValue(
        'specifications',
        specifications.filter((s) => s.id !== specId)
      );
    },
    [specifications, manage]
  );

  const cloneSpecification = useCallback(
    (specId: number) => {
      const spec = specifications.find((s) => s.id === specId);

      if (!spec) {
        return;
      }

      const newId = getNextSpecId(specifications);

      manage.updateValue('specifications', [
        ...specifications,
        {
          ...spec,
          id: newId,
          name: translate('DefaultNameCopiedSpecification', {
            name: spec.name,
          }),
        },
      ]);
    },
    [specifications, manage]
  );

  return {
    ...manage,
    saveCustomFormat: manage.saveProvider,
    specifications,
    saveSpecification,
    deleteSpecification,
    cloneSpecification,
  };
};

export const useBulkEditCustomFormats = (
  onSuccess?: () => void,
  onError?: (error: ApiError) => void
) => {
  const queryClient = useQueryClient();

  const { mutate, isPending, error } = useApiMutation<
    CustomFormat[],
    BulkEditCustomFormatsPayload
  >({
    path: `${PATH}/bulk`,
    method: 'PUT',
    mutationOptions: {
      onSuccess: (updatedCustomFormats) => {
        queryClient.setQueryData<CustomFormat[]>([PATH], (oldCustomFormats) => {
          if (!oldCustomFormats) {
            return oldCustomFormats;
          }

          return oldCustomFormats.map((cf) => {
            const updated = updatedCustomFormats.find((u) => u.id === cf.id);

            return updated ? { ...cf, ...updated } : cf;
          });
        });
        onSuccess?.();
      },
      onError,
    },
  });

  return {
    bulkEditCustomFormats: mutate,
    isSaving: isPending,
    bulkError: error,
  };
};

export const useBulkDeleteCustomFormats = (
  onSuccess?: () => void,
  onError?: (error: ApiError) => void
) => {
  const queryClient = useQueryClient();

  const { mutate, isPending, error } = useApiMutation<
    void,
    BulkDeleteCustomFormatsPayload
  >({
    path: `${PATH}/bulk`,
    method: 'DELETE',
    mutationOptions: {
      onSuccess: (_, variables) => {
        const deletedIds = new Set(variables.ids);

        queryClient.setQueryData<CustomFormat[]>([PATH], (oldCustomFormats) => {
          if (!oldCustomFormats) {
            return oldCustomFormats;
          }

          return oldCustomFormats.filter((cf) => !deletedIds.has(cf.id));
        });
        onSuccess?.();
      },
      onError,
    },
  });

  return {
    bulkDeleteCustomFormats: mutate,
    isDeleting: isPending,
    bulkDeleteError: error,
  };
};
