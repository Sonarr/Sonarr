import { UndefinedInitialDataOptions, useQuery } from '@tanstack/react-query';
import { useMemo } from 'react';

interface ApiErrorResponse {
  message: string;
  details: string;
}

export class ApiError extends Error {
  public statusCode: number;
  public statusText: string;
  public statusBody?: ApiErrorResponse;

  public constructor(
    path: string,
    statusCode: number,
    statusText: string,
    statusBody?: ApiErrorResponse
  ) {
    super(`Request Error: (${statusCode}) ${path}`);

    this.statusCode = statusCode;
    this.statusText = statusText;
    this.statusBody = statusBody;

    Object.setPrototypeOf(this, new.target.prototype);
  }
}

interface QueryOptions<T> {
  path: string;
  headers?: HeadersInit;
  queryOptions?:
    | Omit<UndefinedInitialDataOptions<T, ApiError>, 'queryKey' | 'queryFn'>
    | undefined;
}

const apiRoot = '/api/v5'; // window.Sonarr.apiRoot;

function useApiQuery<T>(options: QueryOptions<T>) {
  const { path, headers } = useMemo(() => {
    return {
      path: apiRoot + options.path,
      headers: {
        ...options.headers,
        'X-Api-Key': window.Sonarr.apiKey,
      },
    };
  }, [options]);

  return useQuery({
    ...options.queryOptions,
    queryKey: [path, headers],
    queryFn: async ({ signal }) => {
      const response = await fetch(path, {
        headers,
        signal,
      });

      if (!response.ok) {
        // eslint-disable-next-line init-declarations
        let body;

        try {
          body = (await response.json()) as ApiErrorResponse;
        } catch {
          throw new ApiError(path, response.status, response.statusText);
        }

        throw new ApiError(path, response.status, response.statusText, body);
      }

      return response.json() as T;
    },
  });
}

export default useApiQuery;
