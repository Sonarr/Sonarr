import React, { createContext, PropsWithChildren, useContext } from 'react';

interface FormRowContextValue {
  id?: string;
  type?: string;
}

const FormRowContext = createContext<FormRowContextValue | undefined>(
  undefined
);

interface FormRowProviderProps extends PropsWithChildren {
  id?: string;
  type?: string;
}

export function FormRowProvider({ id, type, children }: FormRowProviderProps) {
  return (
    <FormRowContext.Provider value={{ id, type }}>
      {children}
    </FormRowContext.Provider>
  );
}

export function useFormRow() {
  return useContext(FormRowContext);
}
