import { useMemo } from 'react';
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import useSeries from 'Series/useSeries';

function useQualityProfileInUse(id: number | undefined) {
  const { data: series = [] } = useSeries();
  const importLists = useSelector(
    (state: AppState) => state.settings.importLists.items
  );

  return useMemo(() => {
    if (!id) {
      return false;
    }

    return (
      series.some((s) => s.qualityProfileId === id) ||
      importLists.some((list) => list.qualityProfileId === id)
    );
  }, [id, series, importLists]);
}

export default useQualityProfileInUse;
