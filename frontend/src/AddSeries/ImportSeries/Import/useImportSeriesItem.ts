import { useMemo } from 'react';
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import { ImportSeries } from 'App/State/ImportSeriesAppState';
import useSeries from 'Series/useSeries';

function useImportSeriesItem(id: string) {
  const { data: series = [] } = useSeries();
  const importSeries = useSelector((state: AppState) => state.importSeries);

  return useMemo(() => {
    const item =
      importSeries.items.find((item) => {
        return item.id === id;
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
  }, [id, importSeries.items, series]);
}

export default useImportSeriesItem;
