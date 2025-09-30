import { useQuery } from '@tanstack/react-query';
import { useMemo } from 'react';
import { PropertyFilter } from 'App/State/AppState';
import { SortDirection } from 'Helpers/Props/sortDirections';
import fetchJson from 'Utilities/Fetch/fetchJson';
import getQueryPath from 'Utilities/Fetch/getQueryPath';
import getQueryString from 'Utilities/Fetch/getQueryString';
import { QueryOptions } from './useApiQuery';

interface PagedQueryOptions<T> extends QueryOptions<PagedQueryResponse<T>> {
  page: number;
  pageSize: number;
  sortKey?: string;
  sortDirection?: SortDirection;
  filters?: PropertyFilter[];
}

interface PagedQueryResponse<T> {
  page: number;
  pageSize: number;
  sortKey: string;
  sortDirection: string;
  totalRecords: number;
  totalPages: number;
  records: T[];
}

const DEFAULT_RECORDS: never[] = [];

const usePagedApiQuery = <T>(options: PagedQueryOptions<T>) => {
  const { requestOptions, queryKey } = useMemo(() => {
    const {
      path,
      page,
      pageSize,
      sortKey,
      sortDirection,
      filters,
      queryParams,
      queryOptions,
      ...otherOptions
    } = options;

    return {
      queryKey: [
        path,
        queryParams,
        page,
        pageSize,
        sortKey,
        sortDirection,
        filters,
      ],
      requestOptions: {
        ...otherOptions,
        path:
          getQueryPath(path) +
          getQueryString({
            ...queryParams,
            page,
            pageSize,
            sortKey,
            sortDirection,
            filters,
          }),
        headers: {
          ...options.headers,
          'X-Api-Key': window.Sonarr.apiKey,
          'X-Sonarr-Client': 'Sonarr',
        },
      },
    };
  }, [options]);

  const { data, ...query } = useQuery({
    ...options.queryOptions,
    queryKey,
    queryFn: async ({ signal }) => {
      const response = await fetchJson<PagedQueryResponse<T>, unknown>({
        ...requestOptions,
        signal,
      });

      return {
        ...response,
        totalPages: Math.max(
          Math.ceil(response.totalRecords / options.pageSize),
          1
        ),
      };
    },
  });

  return {
    ...query,
    records: data?.records ?? DEFAULT_RECORDS,
    totalRecords: data?.totalRecords ?? 0,
    totalPages: data?.totalPages ?? 0,
  };
};

export default usePagedApiQuery;
