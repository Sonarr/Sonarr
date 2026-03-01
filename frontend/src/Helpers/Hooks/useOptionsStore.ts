import { StateCreator } from 'zustand';
import { PersistOptions } from 'zustand/middleware';
import Column from 'Components/Table/Column';
import { createPersist } from 'Helpers/createPersist';
import { SortDirection } from 'Helpers/Props/sortDirections';

type TSettingsWithoutColumns = object;

interface TSettingsWithColumns {
  columns: Column[];
  selectedFilterKey: string | number;
  sortKey: string;
  sortDirection: SortDirection;
}

type TSettings = TSettingsWithoutColumns | TSettingsWithColumns;

export interface PageableOptions {
  pageSize: number;
  selectedFilterKey: string | number;
  sortKey: string;
  sortDirection: SortDirection;
  columns: Column[];
}

export type OptionChanged<T> = {
  name: keyof T;
  value: T[keyof T];
};

export const createOptionsStore = <T extends TSettings>(
  name: string,
  state: StateCreator<T>,
  options: Omit<PersistOptions<T>, 'name' | 'storage'> = {}
) => {
  const store = createPersist<T>(name, state, {
    merge,
    ...options,
  });

  const useOptions = () => {
    return store((state) => state);
  };

  const useOption = <K extends keyof T>(key: K) => {
    return store((state) => state[key]);
  };

  const getOptions = () => {
    return store.getState();
  };

  const getOption = <K extends keyof T>(key: K) => {
    return store.getState()[key];
  };

  const setOptions = (options: Partial<T>) => {
    if ('sortKey' in options || 'sortDirection' in options) {
      throw new Error('Use setSort to set sortKey and sortDirection');
    }

    store.setState((state) => ({
      ...state,
      ...options,
    }));
  };

  const setOption = <K extends keyof T>(key: K, value: T[K]) => {
    if (key === 'sortKey' || key === 'sortDirection') {
      throw new Error('Use setSort to set sortKey and sortDirection');
    }

    store.setState((state) => ({
      ...state,
      [key]: value,
    }));
  };

  const setSort = ({
    sortKey,
    sortDirection,
  }: {
    sortKey: string;
    sortDirection?: SortDirection;
  }) => {
    // @ts-expect-error - Cannot verify if T has sortKey and sortDirection
    store.setState((state) => {
      return applySort(state, sortKey, sortDirection);
    });
  };

  return {
    store,
    useOptions,
    useOption,
    getOptions,
    getOption,
    setOptions,
    setOption,
    setSort,
  };
};

const merge = <T extends TSettings>(
  persistedState: unknown,
  currentState: T
) => {
  if ('columns' in currentState) {
    return {
      ...currentState,
      ...mergeColumns(persistedState, currentState),
    };
  }

  return {
    ...currentState,
    ...((persistedState as T) ?? {}),
  };
};

const mergeColumns = <T extends { columns: Column[] }>(
  persistedState: unknown,
  currentState: T
) => {
  const currentColumns = currentState.columns;
  const persistedColumns = (persistedState as T).columns;
  const columns: Column[] = [];

  // Add persisted columns in the same order they're currently in
  // as long as they haven't been removed.

  persistedColumns.forEach((persistedColumn) => {
    const column = currentColumns.find((i) => i.name === persistedColumn.name);

    if (column) {
      const newColumn: Partial<Column> = {};

      // We can't use a spread operator or Object.assign to clone the column
      // or any accessors are lost and can break translations.
      for (const prop of Object.keys(column)) {
        const attributes = Object.getOwnPropertyDescriptor(column, prop);

        if (!attributes) {
          return;
        }

        Object.defineProperty(newColumn, prop, attributes);
      }

      newColumn.isVisible = persistedColumn.isVisible;

      columns.push(newColumn as Column);
    }
  });

  // Add any columns added to the app in the initial position.
  currentColumns.forEach((currentColumn, index) => {
    const persistedColumnIndex = persistedColumns.findIndex(
      (i) => i.name === currentColumn.name
    );
    const column = Object.assign({}, currentColumn);

    if (persistedColumnIndex === -1) {
      columns.splice(index, 0, column);
    }
  });

  return {
    ...(persistedState as T),
    columns,
  };
};

export const applySort = <T extends TSettings>(
  state: T,
  sortKey: string,
  sortDirection: SortDirection | undefined
) => {
  if (!('sortKey' in state) || !('sortDirection' in state)) {
    return state;
  }

  let newSortDirection = sortDirection;

  if (sortDirection == null) {
    if (state.sortKey === sortKey) {
      newSortDirection =
        state.sortDirection === 'ascending' ? 'descending' : 'ascending';
    } else {
      newSortDirection = state.sortDirection;
    }
  }

  return {
    sortKey,
    sortDirection: newSortDirection,
  };
};
