import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';

function useIsWindows() {
  return useSelector((state: AppState) => state.system.status.item.isWindows);
}

export default useIsWindows;
