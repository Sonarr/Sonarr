import useApiQuery from 'Helpers/Hooks/useApiQuery';
import Task from 'typings/Task';

const useTasks = () => {
  const result = useApiQuery<Task[]>({
    path: '/system/task',
  });

  return {
    ...result,
    data: result.data ?? [],
  };
};

export default useTasks;
