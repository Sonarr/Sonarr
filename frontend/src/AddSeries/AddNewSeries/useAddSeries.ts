import { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import AddSeries from 'AddSeries/AddSeries';
import { AddSeriesOptions } from 'AddSeries/addSeriesOptionsStore';
import useApiMutation from 'Helpers/Hooks/useApiMutation';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import Series from 'Series/Series';
import { updateItem } from 'Store/Actions/baseActions';

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
  const dispatch = useDispatch();

  const onAddSuccess = useCallback(
    (data: Series) => {
      dispatch(updateItem({ section: 'series', ...data }));
    },
    [dispatch]
  );

  return useApiMutation<Series, AddSeriesPayload>({
    path: '/series',
    method: 'POST',
    mutationOptions: {
      onSuccess: onAddSuccess,
    },
  });
};
