import { useQueryClient } from '@tanstack/react-query';
import AddSeries from 'AddSeries/AddSeries';
import { AddSeriesOptions } from 'AddSeries/addSeriesOptionsStore';
import useApiMutation from 'Helpers/Hooks/useApiMutation';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import Series from 'Series/Series';

type AddSeriesPayload = AddSeries & AddSeriesOptions;

export const useLookupSeries = (query: string) => {
  return useApiQuery<AddSeries[]>({
    path: '/series/lookup',
    queryParams: {
      term: query,
    },
    queryOptions: {
      enabled: !!query,
      // Disable refetch on window focus to prevent refetching when the user switch tabs
      refetchOnWindowFocus: false,
    },
  });
};

export const useAddSeries = () => {
  const queryClient = useQueryClient();

  const { isPending, error, mutate } = useApiMutation<Series, AddSeriesPayload>(
    {
      path: '/series',
      method: 'POST',
      mutationOptions: {
        onSuccess: (newSeries) => {
          queryClient.setQueryData<Series[]>(['/series'], (oldSeries) => {
            if (!oldSeries) {
              return [newSeries];
            }

            return [...oldSeries, newSeries];
          });
        },
      },
    }
  );

  return {
    isAdding: isPending,
    addError: error,
    addSeries: mutate,
  };
};
