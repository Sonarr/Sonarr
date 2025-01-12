import { useMemo } from 'react';
import { useLocation } from 'react-router';

function useQueryParams<T>() {
  const { search } = useLocation();

  return useMemo(() => {
    const searchParams = new URLSearchParams(search);

    return searchParams.entries().reduce<T>((acc, [key, value]) => {
      return {
        ...acc,
        [key]: value,
      };
    }, {} as T);
  }, [search]);
}

export default useQueryParams;
