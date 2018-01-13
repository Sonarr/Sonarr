import _ from 'lodash';
import persistState from 'redux-localstorage';
import actions from 'Store/Actions';

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

  initialColumns.forEach((initialColumn) => {
    const persistedColumnIndex = _.findIndex(persistedColumns, { name: initialColumn.name });
    const column = Object.assign({}, initialColumn);
    const persistedColumn = persistedColumnIndex > -1 ? persistedColumns[persistedColumnIndex] : undefined;

    if (persistedColumn) {
      column.isVisible = persistedColumn.isVisible;
    }

    // If there is a persisted column, it's index doesn't exceed the column list
    // and it's modifiable, insert it in the proper position.

    if (persistedColumn && columns.length - 1 > persistedColumnIndex && persistedColumn.isModifiable !== false) {
      columns.splice(persistedColumnIndex, 0, column);
    } else {
      columns.push(column);
    }

    // Set the columns in the persisted state
    _.set(computedState, path, columns);
  });
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

export default persistState(paths, config);
