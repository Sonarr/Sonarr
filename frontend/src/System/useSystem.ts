import { useMutation } from '@tanstack/react-query';
import { useDispatch } from 'react-redux';
import { pingServer, setAppValue } from 'Store/Actions/appActions';

const createSystemMutationFn = (endpoint: string) => {
  return async () => {
    const response = await fetch(
      `${window.Sonarr.urlBase}/system/${endpoint}`,
      {
        method: 'POST',
        headers: {
          'X-Api-Key': window.Sonarr.apiKey,
          'X-Sonarr-Client': 'Sonarr',
        },
      }
    );

    if (!response.ok) {
      throw new Error(`Failed to ${endpoint}: ${response.statusText}`);
    }
  };
};

export const useRestart = () => {
  const dispatch = useDispatch();

  return useMutation<void, Error, void>({
    mutationFn: createSystemMutationFn('restart'),
    onSuccess: () => {
      dispatch(setAppValue({ isRestarting: true }));
      dispatch(pingServer());
    },
  });
};

export const useShutdown = () => {
  return useMutation<void, Error, void>({
    mutationFn: createSystemMutationFn('shutdown'),
  });
};
