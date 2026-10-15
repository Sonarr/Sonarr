import { useMemo } from 'react';
import { useQueueOption } from 'Activity/Queue/queueOptionsStore';
import { FILTERS } from 'Activity/Queue/useQueue';
import { useCustomFiltersList } from 'Filters/useCustomFilters';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import findSelectedFilters from 'Utilities/Filter/findSelectedFilters';

export interface QueueStatus {
  totalCount: number;
  count: number;
  unknownCount: number;
  errors: boolean;
  warnings: boolean;
  unknownErrors: boolean;
  unknownWarnings: boolean;
}

export default function useQueueStatus() {
  const { data } = useApiQuery<QueueStatus>({
    path: '/queue/status',
  });

  const selectedFilterKey = useQueueOption('selectedFilterKey');
  const customFilters = useCustomFiltersList('queue');

  const { includeUnknownSeriesItems } = useMemo(() => {
    const selectedFilters = findSelectedFilters(
      selectedFilterKey,
      FILTERS,
      customFilters
    );

    return selectedFilters.reduce<{
      includeUnknownSeriesItems: boolean;
    }>(
      (acc, filter) => {
        if (
          filter.key === 'includeUnknownSeriesItems' &&
          Array.isArray(filter.value)
        ) {
          acc.includeUnknownSeriesItems = (filter.value as boolean[]).includes(
            true
          );
        }

        return acc;
      },
      {
        includeUnknownSeriesItems: true,
      }
    );
  }, [customFilters, selectedFilterKey]);

  if (!data) {
    return {
      count: 0,
      errors: false,
      warnings: false,
    };
  }

  const {
    errors,
    warnings,
    unknownErrors,
    unknownWarnings,
    count,
    totalCount,
  } = data;

  if (includeUnknownSeriesItems) {
    return {
      count: totalCount,
      errors: errors || unknownErrors,
      warnings: warnings || unknownWarnings,
    };
  }

  return {
    count,
    errors,
    warnings,
  };
}
