import { keepPreviousData } from '@tanstack/react-query';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import { ParseModel } from './ParseModel';

const useParse = (title: string) => {
  const result = useApiQuery<ParseModel>({
    path: '/parse',
    queryParams: { title },
    queryOptions: {
      enabled: title.trim().length > 0,
      placeholderData: keepPreviousData,
    },
  });

  return {
    ...result,
    data: result.data ?? ({} as ParseModel),
  };
};

export default useParse;
