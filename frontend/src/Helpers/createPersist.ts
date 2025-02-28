import { create, type StateCreator } from 'zustand';
import { persist, type PersistOptions } from 'zustand/middleware';
import Column from 'Components/Table/Column';

export const createPersist = <T>(
  name: string,
  state: StateCreator<T>,
  options: Omit<PersistOptions<T>, 'name' | 'storage'> = {}
) => {
  const instanceName =
    window.Sonarr.instanceName.toLowerCase().replace(/ /g, '_') ?? 'sonarr';

  const finalName = `${instanceName}_${name}`;

  return create(
    persist<T>(state, {
      ...options,
      name: finalName,
    })
  );
};

export const mergeColumns = <T extends { columns: Column[] }>(
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
