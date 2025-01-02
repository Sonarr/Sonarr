import useSystemStatus from './useSystemStatus';

function useIsWindowsService() {
  const { isWindows, mode } = useSystemStatus();

  return isWindows && mode === 'service';
}

export default useIsWindowsService;
