import { useCallback, useState } from 'react';

export default function useModalOpenState(
  initialState: boolean
): [boolean, () => void, () => void] {
  const [isOpen, setIsOpen] = useState(initialState);

  const setModalOpen = useCallback(() => {
    setIsOpen(true);
  }, [setIsOpen]);

  const setModalClosed = useCallback(() => {
    setIsOpen(false);
  }, [setIsOpen]);

  return [isOpen, setModalOpen, setModalClosed];
}
