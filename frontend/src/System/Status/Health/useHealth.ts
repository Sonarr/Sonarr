import useApiQuery from 'Helpers/Hooks/useApiQuery';
import Health from 'typings/Health';

const useHealth = () => {
  const result = useApiQuery<Health[]>({
    path: '/health',
  });

  return {
    ...result,
    data: result.data ?? [],
  };
};

export default useHealth;
