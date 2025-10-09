import useSystemStatus from './useSystemStatus';

function useIsContainerized() {
  const { isContainerized = false } = useSystemStatus();
  
  return isContainerized;
}

export default useIsContainerized;
