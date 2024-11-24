import { useQuery } from '@tanstack/react-query';
import { useMemo } from 'react';

interface QueryOptions {
  url: string;
  headers?: HeadersInit;
}

const absUrlRegex = /^(https?:)?\/\//i;
const apiRoot = window.Sonarr.apiRoot;

function isAbsolute(url: string) {
  return absUrlRegex.test(url);
}

function getUrl(url: string) {
  return apiRoot + url;
}

function useApiQuery<T>(options: QueryOptions) {
  const { url, headers } = options;

  const final = useMemo(() => {
    if (isAbsolute(url)) {
      return {
        url,
        headers,
      };
    }

    return {
      url: getUrl(url),
      headers: {
        ...headers,
        'X-Api-Key': window.Sonarr.apiKey,
      },
    };
  }, [url, headers]);

  return useQuery({
    queryKey: [final.url],
    queryFn: async () => {
      const result = await fetch(final.url, {
        headers: final.headers,
      });

      if (!result.ok) {
        throw new Error('Failed to fetch');
      }

      return result.json() as T;
    },
  });
}

export default useApiQuery;
