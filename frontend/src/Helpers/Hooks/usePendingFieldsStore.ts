import { useCallback, useMemo, useState } from 'react';
import { create, useStore } from 'zustand';
import { useShallow } from 'zustand/react/shallow';

interface PendingFieldsStore {
  pendingFields: Map<string, unknown>;
}

export const usePendingFieldsStore = () => {
  // eslint-disable-next-line react/hook-use-state
  const [store] = useState(() => {
    return create<PendingFieldsStore>()((_set) => {
      return {
        pendingFields: new Map(),
      };
    });
  });

  const setPendingField = useCallback(
    (name: string, value: unknown) => {
      store.setState((state) => {
        const newPendingFields = new Map(state.pendingFields);
        newPendingFields.set(name, value);

        return {
          ...state,
          pendingFields: newPendingFields,
        };
      });
    },
    [store]
  );

  const setPendingFields = useCallback(
    (fieldProperties: Record<string, unknown>) => {
      store.setState((state) => {
        const newPendingFields = new Map(state.pendingFields);
        Object.entries(fieldProperties).forEach(([key, value]) => {
          newPendingFields.set(key, value);
        });
        return {
          ...state,
          pendingFields: newPendingFields,
        };
      });
    },
    [store]
  );

  const unsetPendingField = useCallback(
    (name: string) => {
      store.setState((state) => {
        const newPendingFields = new Map(state.pendingFields);
        newPendingFields.delete(name);
        return {
          ...state,
          pendingFields: newPendingFields,
        };
      });
    },
    [store]
  );

  const clearPendingFields = useCallback(() => {
    store.setState((state) => ({
      ...state,
      pendingFields: new Map(),
    }));
  }, [store]);

  const pendingFields = useStore(
    store,
    useShallow((state) => {
      return state.pendingFields;
    })
  );

  const hasPendingFields = useMemo(() => {
    return pendingFields.size > 0;
  }, [pendingFields]);

  return {
    store,
    pendingFields,
    setPendingField,
    setPendingFields,
    unsetPendingField,
    clearPendingFields,
    hasPendingFields,
  };
};
