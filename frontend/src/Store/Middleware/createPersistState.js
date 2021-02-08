import _ from 'lodash';
import persistState from 'redux-localstorage';
import actions from 'Store/Actions';
import migrate from 'Store/Migrators/migrate';

const columnPaths = [];

const paths = _.reduce([...actions], (acc, action) => {
  if (action.persistState) {
    action.persistState.forEach((path) => {
      if (path.match(/\.columns$/)) {
        columnPaths.push(path);
      }

      acc.push(path);
    });
  }

  return acc;
}, []);

function mergeColumns(path, initialState, persistedState, computedState) {
  const initialColumns = _.get(initialState, path);
  const persistedColumns = _.get(persistedState, path);

  if (!persistedColumns || !persistedColumns.length) {
    return;
  }

  const columns = [];

  // Add persisted columns in the same order they're currently in
  // as long as they haven't been removed.

  persistedColumns.forEach((persistedColumn) => {
    const column = initialColumns.find((i) => i.name === persistedColumn.name);

    if (column) {
      columns.push({
        ...column,
        isVisible: persistedColumn.isVisible
      });
    }
  });

  // Add any columns added to the app in the initial position.
  initialColumns.forEach((initialColumn, index) => {
    const persistedColumnIndex = persistedColumns.findIndex((i) => i.name === initialColumn.name);
    const column = Object.assign({}, initialColumn);

    if (persistedColumnIndex === -1) {
      columns.splice(index, 0, column);
    }
  });

  // Set the columns in the persisted state
  _.set(computedState, path, columns);
}

function slicer(paths_) {
  return (state) => {
    const subset = {};

    paths_.forEach((path) => {
      _.set(subset, path, _.get(state, path));
    });

    return subset;
  };
}

function serialize(obj) {
  return JSON.stringify(obj, null, 2);
}

function merge(initialState, persistedState) {
  if (!persistedState) {
    return initialState;
  }

  const computedState = {};

  _.merge(computedState, initialState, persistedState);

  columnPaths.forEach((columnPath) => {
    mergeColumns(columnPath, initialState, persistedState, computedState);
  });

  return computedState;
}

const config = {
  slicer,
  serialize,
  merge,
  key: 'sonarr'
};

export default function createPersistState() {
  // Migrate existing local storage before proceeding
  const persistedState = JSON.parse(localStorage.getItem(config.key));
  migrate(persistedState);
  localStorage.setItem(config.key, serialize(persistedState));

  return persistState(paths, config);
}
