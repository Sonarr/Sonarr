import type { ImportSeriesItem } from './importSeriesStore';

type ImportSeriesEligibilityItem = Pick<
  ImportSeriesItem,
  'id' | 'selectedSeries'
>;

export type ImportSeriesEligibility =
  | 'ready'
  | 'unmatched'
  | 'existing'
  | 'duplicate';

export interface ImportSeriesEligibilityResult {
  eligibilityById: Map<string, ImportSeriesEligibility>;
  duplicateIdsByTvdbId: Map<number, string[]>;
}

export function getImportSeriesEligibility(
  items: ImportSeriesEligibilityItem[],
  existingTvdbIds: Set<number>
): ImportSeriesEligibilityResult {
  const idsByTvdbId = items.reduce<Map<number, string[]>>((ids, item) => {
    const tvdbId = item.selectedSeries?.tvdbId;

    if (tvdbId != null) {
      const existingIds = ids.get(tvdbId);

      if (existingIds) {
        existingIds.push(item.id);
      } else {
        ids.set(tvdbId, [item.id]);
      }
    }

    return ids;
  }, new Map());

  const eligibilityById = items.reduce<Map<string, ImportSeriesEligibility>>(
    (eligibility, item) => {
      const tvdbId = item.selectedSeries?.tvdbId;
      let status: ImportSeriesEligibility = 'unmatched';

      if (tvdbId != null && existingTvdbIds.has(tvdbId)) {
        status = 'existing';
      } else if (tvdbId != null && (idsByTvdbId.get(tvdbId)?.length ?? 0) > 1) {
        status = 'duplicate';
      } else if (tvdbId != null) {
        status = 'ready';
      }

      eligibility.set(item.id, status);

      return eligibility;
    },
    new Map()
  );

  const duplicateIdsByTvdbId = new Map<number, string[]>();

  idsByTvdbId.forEach((ids, tvdbId) => {
    if (ids.length > 1 && !existingTvdbIds.has(tvdbId)) {
      duplicateIdsByTvdbId.set(tvdbId, ids);
    }
  });

  return {
    eligibilityById,
    duplicateIdsByTvdbId,
  };
}
