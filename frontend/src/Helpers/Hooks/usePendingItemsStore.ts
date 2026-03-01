import { useCallback, useMemo, useState } from 'react';
import { create, useStore } from 'zustand';
import { useShallow } from 'zustand/react/shallow';

interface PendingItemsStore<T extends { id: number }> {
  pendingItems: Map<number, Partial<T>>;
}

export const usePendingItemsStore = <T extends { id: number }>() => {
  // eslint-disable-next-line react/hook-use-state
  const [store] = useState(() => {
    return create<PendingItemsStore<T>>()((_set) => {
      return {
        pendingItems: new Map(),
      };
    });
  });

  const setPendingItem = useCallback(
    <K extends keyof T>(id: number, key: K, value: T[K], originalItem?: T) => {
      store.setState((state) => {
        const newPendingItems = new Map(state.pendingItems);
        const existingChanges = newPendingItems.get(id) || {};

        // If the value matches the original, remove it from pending changes
        if (originalItem && originalItem[key] === value) {
          const { [key]: removed, ...rest } = existingChanges;

          if (Object.keys(rest).length === 0) {
            newPendingItems.delete(id);
          } else {
            newPendingItems.set(id, rest);
          }
        } else {
          newPendingItems.set(id, {
            ...existingChanges,
            [key]: value,
          });
        }

        return {
          ...state,
          pendingItems: newPendingItems,
        };
      });
    },
    [store]
  );

  const unsetPendingItem = useCallback(
    (id: number) => {
      store.setState((state) => {
        const newPendingItems = new Map(state.pendingItems);
        newPendingItems.delete(id);

        return {
          ...state,
          pendingItems: newPendingItems,
        };
      });
    },
    [store]
  );

  const clearPendingItems = useCallback(() => {
    store.setState((state) => ({
      ...state,
      pendingItems: new Map(),
    }));
  }, [store]);

  const pendingItems = useStore(
    store,
    useShallow((state) => state.pendingItems)
  );

  const getItemsWithPendingChanges = useCallback(
    (originalItems: T[]): T[] => {
      return originalItems.map((originalItem) => {
        const pendingChanges = pendingItems.get(originalItem.id);

        return pendingChanges
          ? { ...originalItem, ...pendingChanges }
          : originalItem;
      });
    },
    [pendingItems]
  );

  const hasPendingChanges = useMemo(() => {
    return pendingItems.size > 0;
  }, [pendingItems]);

  const getPendingChangesForSave = useCallback(
    (originalItems: T[]): T[] => {
      return originalItems.reduce<T[]>((acc, originalItem) => {
        const pendingChanges = pendingItems.get(originalItem.id);

        if (pendingChanges) {
          acc.push({
            ...originalItem,
            ...pendingChanges,
          });
        }

        return acc;
      }, []);
    },
    [pendingItems]
  );

  return {
    store,
    setPendingItem,
    unsetPendingItem,
    clearPendingItems,
    getItemsWithPendingChanges,
    getPendingChangesForSave,
    hasPendingChanges,
  };
};
