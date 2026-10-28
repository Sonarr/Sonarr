import { createContext, useContext } from 'react';

interface ModalContextValue {
  headerId: string;
  setCloseGuard: (closeGuard: (() => void) | null) => void;
}

export const ModalContext = createContext<ModalContextValue>({
  headerId: '',
  setCloseGuard: () => {},
});

export function useModalContext() {
  return useContext(ModalContext);
}
