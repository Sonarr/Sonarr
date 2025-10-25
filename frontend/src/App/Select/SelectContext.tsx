import React, { createContext, PropsWithChildren } from 'react';
import useSelectStore, {
  Id,
  SelectStoreModel,
} from 'App/Select/useSelectStore';

interface SelectProviderProps<T extends SelectStoreModel<Id>>
  extends PropsWithChildren {
  items: Array<T>;
}

const SelectContext = createContext<
  ReturnType<typeof useSelectStore> | undefined
>(undefined);

export function SelectProvider<T extends SelectStoreModel<Id>>({
  items,
  children,
}: SelectProviderProps<T>) {
  const value = useSelectStore<T>(items);

  return (
    <SelectContext.Provider value={value}>{children}</SelectContext.Provider>
  );
}

export function useSelect<T extends SelectStoreModel<Id>>() {
  const context = React.useContext(SelectContext);

  if (context === undefined) {
    throw new Error('useSelect must be used within a SelectProvider');
  }

  return context as ReturnType<typeof useSelectStore<T>>;
}
