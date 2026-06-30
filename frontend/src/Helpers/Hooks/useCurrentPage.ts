import { useNavigationType } from 'react-router-dom';

function useCurrentPage() {
  const navigationType = useNavigationType();

  return navigationType === 'POP';
}

export default useCurrentPage;
