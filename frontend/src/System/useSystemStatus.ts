import { useSelector } from 'react-redux';
import createSystemStatusSelector from 'Store/Selectors/createSystemStatusSelector';

function useSystemStatus() {
  return useSelector(createSystemStatusSelector());
}

export default useSystemStatus;
