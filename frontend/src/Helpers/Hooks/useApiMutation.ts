import { useMutation, UseMutationOptions } from '@tanstack/react-query';
import { useMemo } from 'react';
import { Error } from 'App/State/AppSectionState';
import fetchJson, { FetchJsonOptions } from 'Utilities/Fetch/fetchJson';
import getQueryPath from 'Utilities/Fetch/getQueryPath';
import getQueryString, { QueryParams } from 'Utilities/Fetch/getQueryString';

interface MutationOptions<T, TData>
  extends Omit<FetchJsonOptions<TData>, 'method'> {
  method: 'POST' | 'PUT' | 'DELETE';
  mutationOptions?: Omit<UseMutationOptions<T, Error, TData>, 'mutationFn'>;
  queryParams?: QueryParams;
}

function useApiMutation<T, TData>(options: MutationOptions<T, TData>) {
  const requestOptions = useMemo(() => {
    return {
      ...options,
      path: getQueryPath(options.path) + getQueryString(options.queryParams),
      headers: {
        ...options.headers,
        'X-Api-Key': window.Sonarr.apiKey,
        'X-Sonarr-Client': 'Sonarr',
      },
    };
  }, [options]);

  return useMutation<T, Error, TData>({
    ...options.mutationOptions,
    mutationFn: async (data?: TData) => {
      const { path, ...otherOptions } = requestOptions;

      return fetchJson<T, TData>({ path, ...otherOptions, body: data });
    },
  });
}

export default useApiMutation;
