import { keepPreviousData, useQuery } from '@tanstack/react-query';
import { useMemo } from 'react';
import { ModelBaseSetting } from 'Store/Selectors/selectSettings';
import Field, { FieldSelectOption } from 'typings/Field';
import fetchJson, { ApiError, urlBase } from 'Utilities/Fetch/fetchJson';

export interface ProviderOptions extends ModelBaseSetting {
  fields?: Field[];
}

export interface ProviderOptionsDevice {
  id: string;
  name: string;
}

export interface ProviderOptionsParams {
  provider: string;
  action: string;
  providerData: ProviderOptions;
}

const importantFieldNames = ['baseUrl', 'apiPath', 'apiKey', 'authToken'];

// Generates a key array based on important provider data fields instead of the whole object.
function getProviderDataKey(providerData: ProviderOptions) {
  if (!providerData || !providerData.fields) {
    return null;
  }

  const fields = providerData.fields
    .filter((f) => importantFieldNames.includes(f.name))
    .map((f) => f.value);

  return fields;
}

function flattenProviderData(providerData: ProviderOptions) {
  return Object.keys(providerData).reduce<Record<string, unknown>>(
    (acc, key) => {
      const property = providerData[key];

      if (key === 'fields') {
        acc[key] = property;
      } else {
        acc[key] = property.value;
      }

      return acc;
    },
    {}
  );
}

export default function useProviderOptions({
  provider,
  action,
  providerData,
}: ProviderOptionsParams) {
  const flattenedData = useMemo(
    () => flattenProviderData(providerData),
    [providerData]
  );

  // TODO: This should be updated to use `useApiQuery`
  const result = useQuery<{ options?: FieldSelectOption<unknown>[] }, ApiError>(
    {
      queryKey: [
        `/${provider}/action/${action}`,
        getProviderDataKey(providerData),
      ],
      enabled: !!(provider && action && providerData),
      queryFn: async ({ signal }) => {
        return fetchJson<
          { options?: FieldSelectOption<unknown>[] },
          typeof flattenedData
        >({
          path: `${urlBase}/api/v3/${provider}/action/${action}`,
          method: 'POST',
          body: flattenedData,
          headers: {
            'Content-Type': 'application/json',
            'X-Api-Key': window.Sonarr.apiKey,
            'X-Sonarr-Client': 'Sonarr',
          },
          signal,
        });
      },
      placeholderData: keepPreviousData,
    }
  );

  return {
    ...result,
    data: result.data?.options || [],
  };
}
