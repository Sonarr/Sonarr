import useSystemStatus from './useSystemStatus';

function useIsContainerized() {
  const { isContainerized } = useSystemStatus();

  return isContainerized ?? false;
}

export default useIsContainerized;
