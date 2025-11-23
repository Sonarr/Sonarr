import { useQueryClient } from '@tanstack/react-query';
import { useMemo } from 'react';
import useApiMutation from 'Helpers/Hooks/useApiMutation';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import { sortByProp } from 'Utilities/Array/sortByProp';
import { CustomFilter } from './Filter';

const DEFAULT_CUSTOM_FILTERS: CustomFilter[] = [];

const useCustomFilters = () => {
  const result = useApiQuery<CustomFilter[]>({
    path: '/customFilter',
  });

  return {
    ...result,
    data: result.data ?? DEFAULT_CUSTOM_FILTERS,
  };
};

export default useCustomFilters;

export const useCustomFiltersList = (type: string) => {
  const { data } = useCustomFilters();

  return useMemo(() => {
    return data.filter((cf) => cf.type === type).sort(sortByProp('label'));
  }, [data, type]);
};

export const useSaveCustomFilter = (id: number | null) => {
  const queryClient = useQueryClient();

  const { mutate, isPending, error, data } = useApiMutation<
    CustomFilter,
    Partial<CustomFilter>
  >({
    path: id === null ? '/customFilter' : `/customFilter/${id}`,
    method: id === null ? 'POST' : 'PUT',
    mutationOptions: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ['/customFilter'] });
      },
    },
  });

  return {
    saveCustomFilter: mutate,
    isSaving: isPending,
    saveError: error,
    newCustomFilter: data,
  };
};

export const useDeleteCustomFilter = (id: number) => {
  const queryClient = useQueryClient();

  const { mutate, isPending, error } = useApiMutation<void, void>({
    path: `/customFilter/${id}`,
    method: 'DELETE',
    mutationOptions: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ['/customFilter'] });
      },
    },
  });

  return {
    deleteCustomFilter: mutate,
    isDeleting: isPending,
    deleteError: error,
  };
};
