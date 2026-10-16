import React, { createContext, PropsWithChildren, useContext } from 'react';

interface FormRowContextValue {
  id?: string;
  name?: string;
  type?: string;
}

const FormRowContext = createContext<FormRowContextValue | undefined>(
  undefined
);

interface FormRowProviderProps extends PropsWithChildren {
  id?: string;
  name?: string;
  type?: string;
}

export function FormRowProvider({
  id,
  name,
  type,
  children,
}: FormRowProviderProps) {
  return (
    <FormRowContext.Provider value={{ id, name, type }}>
      {children}
    </FormRowContext.Provider>
  );
}

export function useFormRow() {
  return useContext(FormRowContext);
}
