import { useCallback, useMemo, useState } from 'react';
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
  // eslint-disable-next-line react/hook-use-state
  const [store] = useState(() => {
    return create<PendingChangesStore<T>>()((_set) => {
      return {
        pendingChanges: initialPendingChanges,
      };
    });
  });

  const setPendingChange = useCallback(
    <K extends keyof T>(key: K, value: T[K]) => {
      store.setState((state) => ({
        ...state,
        pendingChanges: {
          ...state.pendingChanges,
          [key]: value,
        },
      }));
    },
    [store]
  );

  const unsetPendingChange = useCallback(
    <K extends keyof T>(key: K) => {
      store.setState((state) => {
        const newPendingChanges = { ...state.pendingChanges };
        delete newPendingChanges[key];

        return {
          ...state,
          pendingChanges: newPendingChanges,
        };
      });
    },
    [store]
  );

  const clearPendingChanges = useCallback(() => {
    store.setState((state) => ({
      ...state,
      pendingChanges: {},
    }));
  }, [store]);

  const pendingChanges = useStore(
    store,
    useShallow((state) => {
      return state.pendingChanges as Partial<T>;
    })
  );

  const hasPendingChanges = useMemo(() => {
    return Object.keys(pendingChanges).length > 0;
  }, [pendingChanges]);

  return {
    store,
    pendingChanges,
    setPendingChange,
    unsetPendingChange,
    clearPendingChanges,
    hasPendingChanges,
  };
};
