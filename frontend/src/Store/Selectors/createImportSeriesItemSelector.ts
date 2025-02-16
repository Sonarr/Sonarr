import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import { ImportSeries } from 'App/State/ImportSeriesAppState';
import createAllSeriesSelector from './createAllSeriesSelector';

function createImportSeriesItemSelector(id: string) {
  return createSelector(
    (_state: AppState, connectorInput: { id: string }) =>
      connectorInput ? connectorInput.id : id,
    (state: AppState) => state.importSeries,
    createAllSeriesSelector(),
    (connectorId, importSeries, series) => {
      const finalId = id || connectorId;

      const item =
        importSeries.items.find((item) => {
          return item.id === finalId;
        }) ?? ({} as ImportSeries);

      const selectedSeries = item && item.selectedSeries;
      const isExistingSeries =
        !!selectedSeries &&
        series.some((s) => {
          return s.tvdbId === selectedSeries.tvdbId;
        });

      return {
        ...item,
        isExistingSeries,
      };
    }
  );
}

export default createImportSeriesItemSelector;
