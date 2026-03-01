import { useMemo } from 'react';
import useSeries from 'Series/useSeries';

function useExistingSeries(tvdbId: number | undefined) {
  const { data: series = [] } = useSeries();

  return useMemo(() => {
    if (tvdbId == null) {
      return false;
    }

    return series.some((s) => s.tvdbId === tvdbId);
  }, [tvdbId, series]);
}

export default useExistingSeries;
