import { isEqual } from 'lodash';
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

function areValuesEqual(value: unknown, originalValue: unknown) {
  if (Array.isArray(value) && Array.isArray(originalValue)) {
    return isEqual(new Set(value), new Set(originalValue));
  }

  return isEqual(value, originalValue);
}

export const usePendingChangesStore = <T extends object>(
  initialPendingChanges: Partial<T>,
  originalValues?: T
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
    const keys = Object.keys(pendingChanges) as (keyof T)[];

    if (!originalValues) {
      return keys.length > 0;
    }

    return keys.some(
      (key) => !areValuesEqual(pendingChanges[key], originalValues[key])
    );
  }, [pendingChanges, originalValues]);

  return {
    store,
    pendingChanges,
    setPendingChange,
    clearPendingChanges,
    hasPendingChanges,
  };
};
