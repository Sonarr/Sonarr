import { useCallback, useEffect, useRef, useState } from 'react';
import { create, useStore } from 'zustand';
import { useShallow } from 'zustand/react/shallow';
import getToggledRange from 'Utilities/Table/getToggledRange';

export type Id = string | number;
export type SelectStoreReturnType<T extends SelectStoreModel<Id>> = ReturnType<
  typeof useSelectStore<T>
>;

type ItemState<T extends SelectStoreModel<Id>> = Map<T['id'], ItemStateValue>;

interface ItemStateValue {
  isSelected: boolean;
  isDisabled?: boolean;
}

export interface SelectStoreModel<TId extends Id> {
  id: TId;
}

export interface SelectStore<T extends SelectStoreModel<Id>> {
  itemState: Map<T['id'], ItemStateValue>;
  lastToggled: T['id'] | null;
  items: T[];
}

interface ItemSelectState {
  allSelected: boolean;
  allUnselected: boolean;
  anySelected: boolean;
  selectedCount: number;
}

const initialState = <T extends SelectStoreModel<Id>>(
  items: T[] = []
): SelectStore<T> => ({
  itemState: new Map<T['id'], ItemStateValue>(),
  lastToggled: null,
  items,
});

function toggleAll<T extends SelectStoreModel<Id>>(
  itemState: ItemState<T>,
  isSelected: boolean
) {
  const newItemState = new Map(itemState);

  newItemState.forEach((value, key) => {
    newItemState.set(key, {
      isSelected: value.isDisabled ? value.isSelected : isSelected,
      isDisabled: value.isDisabled,
    });
  });

  return newItemState;
}

export default function useSelectStore<T extends SelectStoreModel<Id>>(
  items: SelectStoreModel<T['id']>[]
) {
  const store = useRef(
    create<SelectStore<T>>(() => initialState(items as T[]))
  );

  const [itemSelectState, setItemSelectState] = useState<ItemSelectState>({
    allSelected: false,
    allUnselected: true,
    anySelected: false,
    selectedCount: 0,
  });

  const reset = useCallback(() => {
    store.current.setState(initialState(items as T[]), true);
  }, [items]);

  const selectAll = useCallback(() => {
    store.current.setState((state) => {
      const newItemState = toggleAll(state.itemState, true);

      return {
        lastToggled: null,
        itemState: newItemState,
      };
    });
  }, []);

  const unselectAll = useCallback(() => {
    store.current.setState((state) => {
      const newItemState = toggleAll(state.itemState, false);

      return {
        lastToggled: null,
        itemState: newItemState,
      };
    });
  }, []);

  const toggleSelected = useCallback(
    ({
      id,
      isSelected,
      shiftKey,
    }: {
      id: T['id'];
      isSelected: boolean | null;
      shiftKey: boolean;
    }) => {
      store.current.setState((state) => {
        const lastToggled = state.lastToggled;
        const nextSelectedState = new Map(state.itemState);
        const currentItemState = nextSelectedState.get(id);

        if (isSelected == null) {
          nextSelectedState.delete(id);
        } else if (!currentItemState?.isDisabled) {
          nextSelectedState.set(id, {
            isSelected,
            isDisabled: currentItemState?.isDisabled,
          });

          if (shiftKey && lastToggled) {
            const { lower, upper } = getToggledRange(
              state.items,
              id,
              lastToggled
            );

            for (let i = lower; i < upper; i++) {
              if (!nextSelectedState.get(state.items[i].id)?.isDisabled) {
                nextSelectedState.set(state.items[i].id, {
                  isSelected,
                  isDisabled: currentItemState?.isDisabled,
                });
              }
            }
          }
        }

        return {
          ...state,
          lastToggled: id,
          itemState: nextSelectedState,
        };
      });
    },
    []
  );

  const toggleDisabled = useCallback((id: T['id'], isDisabled: boolean) => {
    store.current.setState((state) => {
      const currentItemState = state.itemState.get(id);

      if (currentItemState) {
        const newItemState = new Map(state.itemState);
        newItemState.set(id, {
          ...currentItemState,
          isDisabled,
        });

        return {
          itemState: newItemState,
        };
      }

      return state;
    });
  }, []);

  const getSelectedIds = useCallback((): Array<T['id']> => {
    const iState = store.current.getState().itemState;

    return Array.from(iState.entries()).reduce<T['id'][]>(
      (acc, [id, value]) => {
        if (value.isSelected) {
          acc.push(id);
        }
        return acc;
      },
      []
    );
  }, []);

  const getIsSelected = useCallback((id: T['id']): boolean => {
    const item = store.current.getState().itemState.get(id);

    return item?.isSelected ?? false;
  }, []);

  const useIsSelected = (id: T['id']) => {
    return useStore(
      store.current,
      useShallow((state) => {
        const item = state.itemState.get(id);

        return item?.isSelected ?? false;
      })
    );
  };

  const useSelectedIds = () => {
    return useStore(
      store.current,
      useShallow((state) => {
        return state.itemState
          .entries()
          .reduce<T['id'][]>((acc, [id, value]) => {
            if (value.isSelected) {
              acc.push(id);
            }
            return acc;
          }, []);
      })
    );
  };

  const useHasItems = () => {
    return useStore(
      store.current,
      useShallow((state) => {
        return state.itemState.size > 0;
      })
    );
  };

  useEffect(() => {
    const unsubscribe = store.current.subscribe((state) => {
      const itemState = state.itemState;

      const { allSelected, allUnselected, anySelected, selectedCount } =
        itemState.values().reduce(
          (acc, item) => {
            acc.allSelected =
              acc.allSelected && !!(item.isSelected || item.isDisabled);
            acc.allUnselected =
              acc.allUnselected && (!item.isSelected || !!item.isDisabled);
            acc.anySelected = acc.anySelected || item.isSelected;
            acc.selectedCount += item.isSelected ? 1 : 0;

            return acc;
          },
          {
            allSelected:
              itemState.size > 0 &&
              itemState.values().some((i) => i.isSelected),
            allUnselected: true,
            anySelected: false,
            selectedCount: 0,
          }
        );

      setItemSelectState((s) => {
        if (
          s.allSelected === allSelected &&
          s.allUnselected === allUnselected &&
          s.anySelected === anySelected &&
          s.selectedCount === selectedCount
        ) {
          return s;
        }

        return {
          allSelected,
          allUnselected,
          anySelected,
          selectedCount,
        };
      });
    });

    return () => {
      unsubscribe();
    };
  }, []);

  useEffect(() => {
    store.current.setState((state) => {
      const nextItemState = items.reduce((acc: ItemState<T>, item) => {
        const id = item.id;
        const existingItem = state.itemState.get(id);

        acc.set(
          id,
          existingItem ?? {
            isSelected: false,
            isDisabled: false,
          }
        );

        return acc;
      }, new Map<T['id'], ItemStateValue>());

      return {
        itemState: nextItemState,
        lastToggled: null,
        items: items as T[],
      };
    });
  }, [items]);

  return {
    ...itemSelectState,
    getIsSelected,
    getSelectedIds,
    reset,
    selectAll,
    toggleDisabled,
    toggleSelected,
    unselectAll,
    useHasItems,
    useIsSelected,
    useSelectedIds,
  };
}
