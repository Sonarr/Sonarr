import { UndefinedInitialDataOptions, useQuery } from '@tanstack/react-query';
import { useMemo } from 'react';
import fetchJson, {
  ApiError,
  apiRoot,
  FetchJsonOptions,
} from 'Utilities/Fetch/fetchJson';

interface QueryOptions<T> extends FetchJsonOptions<unknown> {
  queryOptions?:
    | Omit<UndefinedInitialDataOptions<T, ApiError>, 'queryKey' | 'queryFn'>
    | undefined;
}

function useApiQuery<T>(options: QueryOptions<T>) {
  const requestOptions = useMemo(() => {
    const { queryOptions, ...otherOptions } = options;

    return {
      ...otherOptions,
      path: apiRoot + options.path,
      headers: {
        ...options.headers,
        'X-Api-Key': window.Sonarr.apiKey,
      },
    };
  }, [options]);

  return useQuery({
    ...options.queryOptions,
    queryKey: [requestOptions.path],
    queryFn: async ({ signal }) =>
      fetchJson<T, unknown>({ ...requestOptions, signal }),
  });
}

export default useApiQuery;
