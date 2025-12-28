import { useQueryClient } from '@tanstack/react-query';
import AddSeries from 'AddSeries/AddSeries';
import { AddSeriesOptions } from 'AddSeries/addSeriesOptionsStore';
import useApiMutation from 'Helpers/Hooks/useApiMutation';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import Series from 'Series/Series';

interface AddSeriesPayload
  extends AddSeries,
    Omit<
      AddSeriesOptions,
      'monitor' | 'searchForMissingEpisodes' | 'searchForCutoffUnmetEpisodes'
    > {}

const DEFAULT_SERIES: AddSeries[] = [];

export const useLookupSeries = (query: string, isEnabled = true) => {
  const result = useApiQuery<AddSeries[]>({
    path: '/series/lookup',
    queryParams: {
      term: query,
    },
    queryOptions: {
      enabled: isEnabled && !!query,
      // Disable refetch on window focus to prevent refetching when the user switch tabs
      refetchOnWindowFocus: false,
    },
  });

  return {
    ...result,
    data: result.data ?? DEFAULT_SERIES,
  };
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
