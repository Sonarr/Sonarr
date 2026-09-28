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

export interface ImportSeriesSelectionResult {
  importableIds: string[];
  duplicateIdsToDeselect: Set<string>;
}

export function getImportSeriesSelection(
  { eligibilityById, duplicateIdsByTvdbId }: ImportSeriesEligibilityResult,
  selectedIds: string[]
): ImportSeriesSelectionResult {
  const selectedIdSet = new Set(selectedIds);
  const chosenDuplicateIds = new Set<string>();
  const duplicateIdsToDeselect = new Set<string>();

  duplicateIdsByTvdbId.forEach((duplicateIds) => {
    let hasChosenId = false;

    duplicateIds.forEach((duplicateId) => {
      if (!selectedIdSet.has(duplicateId)) {
        return;
      }

      if (hasChosenId) {
        duplicateIdsToDeselect.add(duplicateId);
      } else {
        chosenDuplicateIds.add(duplicateId);
        hasChosenId = true;
      }
    });
  });

  const importableIds = selectedIds.filter((id) => {
    const status = eligibilityById.get(id);

    return status === 'ready' || chosenDuplicateIds.has(id);
  });

  return { importableIds, duplicateIdsToDeselect };
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
