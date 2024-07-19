import { useHistory } from 'react-router-dom';

function useCurrentPage() {
  const history = useHistory();

  return history.action === 'POP';
}

export default useCurrentPage;
