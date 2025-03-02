import useApiQuery from 'Helpers/Hooks/useApiQuery';
import Update from 'typings/Update';

const useUpdates = () => {
  const result = useApiQuery<Update[]>({
    path: '/update',
  });

  return {
    ...result,
    data: result.data ?? [],
  };
};

export default useUpdates;
