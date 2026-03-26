import { createContext, useContext } from 'react';

interface ModalContextValue {
  headerId: string;
}

export const ModalContext = createContext<ModalContextValue>({ headerId: '' });

export function useModalContext() {
  return useContext(ModalContext);
}
