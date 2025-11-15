import React, { createContext, PropsWithChildren, useMemo } from 'react';
import { ValidationError, ValidationWarning } from 'typings/pending';
import { ValidationMessage } from './FormInputGroup';

interface FormInputGroupProviderProps extends PropsWithChildren {
  setClientErrors: (errors: (ValidationMessage | ValidationError)[]) => void;
  setClientWarnings: (
    warnings: (ValidationMessage | ValidationWarning)[]
  ) => void;
}

interface FormInputGroupContextProps {
  setClientErrors: (errors: (ValidationMessage | ValidationError)[]) => void;
  setClientWarnings: (
    warnings: (ValidationMessage | ValidationWarning)[]
  ) => void;
}

const FormInputGroupContext = createContext<
  FormInputGroupContextProps | undefined
>(undefined);

export function FormInputGroupProvider({
  setClientErrors,
  setClientWarnings,
  children,
}: FormInputGroupProviderProps) {
  const value = useMemo(() => {
    return {
      setClientErrors,
      setClientWarnings,
    };
  }, [setClientErrors, setClientWarnings]);

  return (
    <FormInputGroupContext.Provider value={value}>
      {children}
    </FormInputGroupContext.Provider>
  );
}

export function useFormInputGroup() {
  return React.useContext(FormInputGroupContext);
}
