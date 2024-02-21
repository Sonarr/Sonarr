import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

const createIndexerFlagsSelector = createSelector(
  (state: AppState) => state.settings.indexerFlags,
  (indexerFlags) => indexerFlags
);

export default createIndexerFlagsSelector;
