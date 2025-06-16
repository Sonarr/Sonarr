import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import { SelectedState } from 'Helpers/Hooks/useSelectState';
import createAllSeriesSelector from './createAllSeriesSelector';
import { isExistingSeries } from './createExistingSeriesSelector';

function createImportSeriesAllSelectedSelector() {
  return createSelector(
    (state: AppState) => state.importSeries.items,
    createAllSeriesSelector(),
    (_state: AppState, selectedState: SelectedState) => selectedState,
    (importSeries, allSeries, selectedState) => {
      const selectableImportSeries = importSeries.filter((item) => {
        if (!item.selectedSeries) {
          return false;
        }

        return !isExistingSeries(allSeries, item.selectedSeries.tvdbId);
      });

      return (
        selectableImportSeries.reduce((acc, item) => {
          return acc && selectedState[item.id];
        }, true) && selectableImportSeries.length > 0
      );
    }
  );
}

export default createImportSeriesAllSelectedSelector;
