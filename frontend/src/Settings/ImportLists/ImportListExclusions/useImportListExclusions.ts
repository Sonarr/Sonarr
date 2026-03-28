import { keepPreviousData, useQueryClient } from '@tanstack/react-query';
import { useCallback, useMemo } from 'react';
import ModelBase from 'App/ModelBase';
import useApiMutation from 'Helpers/Hooks/useApiMutation';
import usePage from 'Helpers/Hooks/usePage';
import usePagedApiQuery from 'Helpers/Hooks/usePagedApiQuery';
import { usePendingChangesStore } from 'Helpers/Hooks/usePendingChangesStore';
import selectSettings from 'Store/Selectors/selectSettings';
import { useImportListExclusionOptions } from './importListExclusionOptionsStore';

export interface ImportListExclusion extends ModelBase {
  tvdbId: number;
  title: string;
}

const PATH = '/importlistexclusion';

const NEW_IMPORT_LIST_EXCLUSION = {
  title: '',
  tvdbId: 0,
};

interface BulkImportListExclusionData {
  ids: number[];
}

const useImportListExclusions = () => {
  const { page, goToPage } = usePage('importListExclusion');
  const { pageSize, sortKey, sortDirection } = useImportListExclusionOptions();

  const { refetch, ...query } = usePagedApiQuery<ImportListExclusion>({
    path: PATH,
    page,
    pageSize,
    sortKey,
    sortDirection,
    queryOptions: {
      placeholderData: keepPreviousData,
    },
  });

  return {
    ...query,
    goToPage,
    page,
    refetch,
  };
};

export default useImportListExclusions;

interface ManageImportListExclusionOptions {
  id?: number;
  title?: string;
  tvdbId?: number;
}

export const useManageImportListExclusion = ({
  id,
  title,
  tvdbId,
}: ManageImportListExclusionOptions) => {
  const queryClient = useQueryClient();

  const item = useMemo(() => {
    return id
      ? { title: title ?? '', tvdbId: tvdbId ?? 0 }
      : NEW_IMPORT_LIST_EXCLUSION;
  }, [id, title, tvdbId]);

  const { pendingChanges, setPendingChange } =
    usePendingChangesStore<ImportListExclusion>({});

  const {
    mutate,
    isPending: isSaving,
    error: saveError,
  } = useApiMutation<ImportListExclusion, ImportListExclusion>({
    path: id ? `${PATH}/${id}` : PATH,
    method: id ? 'PUT' : 'POST',
    mutationOptions: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: [PATH] });
      },
    },
  });

  const { settings, validationErrors, validationWarnings } = useMemo(() => {
    return selectSettings(item, pendingChanges, saveError);
  }, [item, pendingChanges, saveError]);

  const updateValue = useCallback(
    (name: string, value: unknown) => {
      // @ts-expect-error - name is not yet typed
      setPendingChange(name, value);
    },
    [setPendingChange]
  );

  const save = useCallback(() => {
    const payload = {
      ...item,
      ...pendingChanges,
    } as ImportListExclusion;

    if (id) {
      payload.id = id;
    }

    mutate(payload);
  }, [id, item, pendingChanges, mutate]);

  return {
    item: settings,
    isSaving,
    saveError,
    validationErrors,
    validationWarnings,
    updateValue,
    save,
  };
};

export const useDeleteImportListExclusion = (id: number) => {
  const queryClient = useQueryClient();

  const { mutate, isPending } = useApiMutation<unknown, void>({
    path: `${PATH}/${id}`,
    method: 'DELETE',
    mutationOptions: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: [PATH] });
      },
    },
  });

  return {
    deleteImportListExclusion: mutate,
    isDeleting: isPending,
  };
};

export const useDeleteImportListExclusions = () => {
  const queryClient = useQueryClient();

  const { mutate, isPending } = useApiMutation<
    unknown,
    BulkImportListExclusionData
  >({
    path: `${PATH}/bulk`,
    method: 'DELETE',
    mutationOptions: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: [PATH] });
      },
    },
  });

  return {
    deleteImportListExclusions: mutate,
    isDeleting: isPending,
  };
};
