import { useCallback, useState } from 'react';

export default function useModalOpenState(
  initialState: boolean
): [boolean, () => void, () => void] {
  const [isOpen, setOpen] = useState(initialState);

  const setModalOpen = useCallback(() => {
    setOpen(true);
  }, [setOpen]);

  const setModalClosed = useCallback(() => {
    setOpen(false);
  }, [setOpen]);

  return [isOpen, setModalOpen, setModalClosed];
}
