import { useMutation, UseMutationOptions } from '@tanstack/react-query';
import { useMemo } from 'react';
import { Error } from 'App/State/AppSectionState';
import fetchJson, {
  apiRoot,
  FetchJsonOptions,
} from 'Utilities/Fetch/fetchJson';

interface MutationOptions<T, TData>
  extends Omit<FetchJsonOptions<TData>, 'method'> {
  method: 'POST' | 'PUT' | 'DELETE';
  mutationOptions?: Omit<UseMutationOptions<T, Error, TData>, 'mutationFn'>;
}

function useApiMutation<T, TData>(options: MutationOptions<T, TData>) {
  const requestOptions = useMemo(() => {
    return {
      ...options,
      path: apiRoot + options.path,
      headers: {
        ...options.headers,
        'X-Api-Key': window.Sonarr.apiKey,
      },
    };
  }, [options]);

  return useMutation<T, Error, TData>({
    ...options.mutationOptions,
    mutationFn: async (data: TData) =>
      fetchJson<T, TData>({ ...requestOptions, body: data }),
  });
}

export default useApiMutation;
