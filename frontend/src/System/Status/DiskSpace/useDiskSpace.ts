import useApiQuery from 'Helpers/Hooks/useApiQuery';
import DiskSpace from 'typings/DiskSpace';

const useDiskSpace = () => {
  const result = useApiQuery<DiskSpace[]>({
    path: '/diskspace',
  });

  return {
    ...result,
    data: result.data ?? [],
  };
};

export default useDiskSpace;
