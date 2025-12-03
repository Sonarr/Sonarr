import { pingServer, setAppValue } from 'App/appStore';
import useApiMutation from 'Helpers/Hooks/useApiMutation';

export const useRestart = () => {
  const mutation = useApiMutation<void, void>({
    method: 'POST',
    path: '/system/restart',
  });

  const restart = () => {
    mutation.mutate(undefined, {
      onSuccess: () => {
        setAppValue({ isRestarting: true });
        pingServer();
      },
    });
  };

  return {
    ...mutation,
    mutate: restart,
  };
};

export const useShutdown = () => {
  return useApiMutation<void, void>({
    method: 'POST',
    path: '/system/shutdown',
  });
};
