import React, { createContext, PropsWithChildren, useMemo } from 'react';
import { ValidationError, ValidationWarning } from 'typings/pending';
import type { ValidationMessage } from './FormInput';

interface FormInputGroupProviderProps extends PropsWithChildren {
  describedBy?: string;
  hasError: boolean;
  setClientErrors: (errors: (ValidationMessage | ValidationError)[]) => void;
  setClientWarnings: (
    warnings: (ValidationMessage | ValidationWarning)[]
  ) => void;
}

interface FormInputGroupContextProps {
  describedBy?: string;
  hasError: boolean;
  setClientErrors: (errors: (ValidationMessage | ValidationError)[]) => void;
  setClientWarnings: (
    warnings: (ValidationMessage | ValidationWarning)[]
  ) => void;
}

const FormInputGroupContext = createContext<
  FormInputGroupContextProps | undefined
>(undefined);

export function FormInputGroupProvider({
  describedBy,
  hasError,
  setClientErrors,
  setClientWarnings,
  children,
}: FormInputGroupProviderProps) {
  const value = useMemo(() => {
    return {
      describedBy,
      hasError,
      setClientErrors,
      setClientWarnings,
    };
  }, [describedBy, hasError, setClientErrors, setClientWarnings]);

  return (
    <FormInputGroupContext.Provider value={value}>
      {children}
    </FormInputGroupContext.Provider>
  );
}

export function useFormInputGroup() {
  return React.useContext(FormInputGroupContext);
}
