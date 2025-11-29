import { useMemo, useRef } from 'react';
import { create, useStore } from 'zustand';
import { useShallow } from 'zustand/react/shallow';

export type PendingChanged<T> = {
  name: keyof T;
  value: T[keyof T];
};

interface PendingChangesStore<T extends object> {
  pendingChanges: Partial<T>;
}

export const usePendingChangesStore = <T extends object>(
  initialPendingChanges: Partial<T>
) => {
  const store = useRef(
    create<PendingChangesStore<T>>()((_set) => {
      return {
        pendingChanges: initialPendingChanges,
      };
    })
  );

  const usePendingChanges = () => {
    return useStore(
      store.current,
      useShallow((state) => {
        return state.pendingChanges as Partial<T>;
      })
    );
  };

  const setPendingChange = <K extends keyof T>(key: K, value: T[K]) => {
    store.current.setState((state) => ({
      ...state,
      pendingChanges: {
        ...state.pendingChanges,
        [key]: value,
      },
    }));
  };

  const setPendingChanges = (changes: Partial<T>) => {
    store.current.setState((state) => ({
      ...state,
      pendingChanges: {
        ...state.pendingChanges,
        ...changes,
      },
    }));
  };

  const discardPendingChanges = () => {
    return setPendingChanges({} as Partial<T>);
  };

  const pendingChanges = usePendingChanges();

  const hasPendingChanges = useMemo(() => {
    return Object.keys(pendingChanges).length > 0;
  }, [pendingChanges]);

  return {
    store,
    pendingChanges,
    setPendingChange,
    discardPendingChanges,
    hasPendingChanges,
  };
};
