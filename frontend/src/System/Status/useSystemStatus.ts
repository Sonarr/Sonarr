import useApiQuery from 'Helpers/Hooks/useApiQuery';
import SystemStatus from 'typings/SystemStatus';

const useSystemStatus = () => {
  const result = useApiQuery<SystemStatus>({
    path: '/system/status',
    queryOptions: {
      staleTime: 30 * 1000,
    },
  });

  return {
    ...result,
    data: result.data ?? ({} as SystemStatus),
  };
};

export default useSystemStatus;

export const useSystemStatusData = () => {
  const { data } = useSystemStatus();

  return data;
};

export const useIsWindows = () => {
  const { isWindows } = useSystemStatusData();

  return isWindows;
};

export const useIsWindowsService = () => {
  const { isWindows, mode } = useSystemStatusData();

  return isWindows && mode === 'service';
};
