import React, {
  createContext,
  PropsWithChildren,
  useCallback,
  useContext,
  useMemo,
  useState,
} from 'react';

interface FormRowContextValue {
  id?: string;
  type?: string;
  descriptionIds: string[];
  addDescriptionId: (id: string) => void;
  removeDescriptionId: (id: string) => void;
}

const FormRowContext = createContext<FormRowContextValue | undefined>(
  undefined
);

interface FormRowProviderProps extends PropsWithChildren {
  id?: string;
  type?: string;
}

export function FormRowProvider({ id, type, children }: FormRowProviderProps) {
  const [descriptionIds, setDescriptionIds] = useState<string[]>([]);

  const addDescriptionId = useCallback((descriptionId: string) => {
    setDescriptionIds((currentIds) => {
      return currentIds.includes(descriptionId)
        ? currentIds
        : [...currentIds, descriptionId];
    });
  }, []);

  const removeDescriptionId = useCallback((descriptionId: string) => {
    setDescriptionIds((currentIds) => {
      return currentIds.filter((currentId) => currentId !== descriptionId);
    });
  }, []);

  const value = useMemo(() => {
    return {
      id,
      type,
      descriptionIds,
      addDescriptionId,
      removeDescriptionId,
    };
  }, [id, type, descriptionIds, addDescriptionId, removeDescriptionId]);

  return (
    <FormRowContext.Provider value={value}>{children}</FormRowContext.Provider>
  );
}

export function useFormRow() {
  return useContext(FormRowContext);
}
