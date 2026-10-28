import { useEffect } from 'react';
import { useModalContext } from 'Components/Modal/ModalContext';

function useModalCloseGuard(onClose: () => void) {
  const { setCloseGuard } = useModalContext();

  useEffect(() => {
    setCloseGuard(onClose);

    return () => setCloseGuard(null);
  }, [setCloseGuard, onClose]);
}

export default useModalCloseGuard;
