import { useMemo } from 'react';
import useSeries from 'Series/useSeries';

function useExistingSeries(tvdbId: number | undefined) {
  const { data: series = [] } = useSeries();

  return useMemo(() => {
    if (tvdbId == null) {
      return undefined;
    }

    return series.find((s) => s.tvdbId === tvdbId);
  }, [tvdbId, series]);
}

export default useExistingSeries;
