import useApiQuery from 'Helpers/Hooks/useApiQuery';

export interface IndexerFlag {
  id: number;
  name: string;
}

const DEFAULT_INDEXER_FLAGS: IndexerFlag[] = [];

const useIndexerFlags = () => {
  const result = useApiQuery<IndexerFlag[]>({
    path: '/indexerFlag',
    queryOptions: {
      gcTime: Infinity,
      staleTime: Infinity,
    },
  });

  return {
    ...result,
    data: result.data ?? DEFAULT_INDEXER_FLAGS,
  };
};

export default useIndexerFlags;
