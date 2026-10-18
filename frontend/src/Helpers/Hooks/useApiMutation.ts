import { useMutation, UseMutationOptions } from '@tanstack/react-query';
import { useMemo } from 'react';
import ModelBase from 'App/ModelBase';
import {
  ValidationError,
  ValidationFailure,
  ValidationWarning,
} from 'typings/pending';
import fetchJson, {
  ApiError,
  FetchJsonOptions,
} from 'Utilities/Fetch/fetchJson';
import getQueryPath from 'Utilities/Fetch/getQueryPath';
import getQueryString, { QueryParams } from 'Utilities/Fetch/getQueryString';
import { ValidationFailures } from 'Utilities/selectSettings';

interface MutationRequest<TData> {
  body?: TData;
  queryParams?: QueryParams;
}

interface MutationOptions<T, TData, TVariables = TData>
  extends Omit<FetchJsonOptions<TData>, 'method'> {
  method: 'POST' | 'PUT' | 'DELETE';
  mutationOptions?: Omit<
    UseMutationOptions<T, ApiError, TVariables>,
    'mutationFn'
  >;
  queryParams?: QueryParams;
  getRequest?: (variables: TVariables) => MutationRequest<TData>;
}

function useApiMutation<T, TData, TVariables = TData>(
  options: MutationOptions<T, TData, TVariables>
) {
  const requestOptions = useMemo(() => {
    const { mutationOptions, queryParams, getRequest, ...otherOptions } =
      options;

    return {
      ...otherOptions,
      path: getQueryPath(options.path),
      headers: {
        ...options.headers,
        'X-Api-Key': window.Sonarr.apiKey,
        'X-Sonarr-Client': 'Sonarr',
      },
    };
  }, [options]);

  return useMutation<T, ApiError, TVariables>({
    ...options.mutationOptions,
    mutationFn: async (variables: TVariables) => {
      const { path, ...otherOptions } = requestOptions;
      const { body, queryParams } = options.getRequest
        ? options.getRequest(variables)
        : { body: variables as unknown as TData, queryParams: undefined };

      return fetchJson<T, TData>({
        ...otherOptions,
        path: path + getQueryString({ ...options.queryParams, ...queryParams }),
        body,
      });
    },
  });
}

export default useApiMutation;

export function getValidationFailures(
  error?: ApiError | null
): ValidationFailures {
  if (!error || error.statusCode !== 400 || !Array.isArray(error.statusBody)) {
    return {
      errors: [],
      warnings: [],
    };
  }

  return (error.statusBody as ValidationFailure[]).reduce(
    (acc: ValidationFailures, failure: ValidationFailure) => {
      if (failure.isWarning) {
        acc.warnings.push(failure as ValidationWarning);
      } else {
        acc.errors.push(failure as ValidationError);
      }

      return acc;
    },
    {
      errors: [],
      warnings: [],
    }
  );
}

export function addOrUpdateQueryClientItem<
  T extends ModelBase,
  K extends keyof T
>(oldData: T[] = [], newItem: T, key: K) {
  const existingIndex = oldData.findIndex((item) => item[key] === newItem[key]);

  if (existingIndex === -1) {
    return [...oldData, newItem];
  }

  return oldData.map((item) => (item[key] === newItem[key] ? newItem : item));
}
