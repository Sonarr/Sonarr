import useApiQuery from 'Helpers/Hooks/useApiQuery';

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

  if (!data) {
    return {
      count: 0,
      errors: false,
      warnings: false,
    };
  }

  const { errors, warnings, unknownErrors, unknownWarnings, totalCount } = data;

  return {
    count: totalCount,
    errors: errors || unknownErrors,
    warnings: warnings || unknownWarnings,
  };
}
