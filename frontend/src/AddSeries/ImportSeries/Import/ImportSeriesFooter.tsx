import React, { useCallback, useMemo } from 'react';
import { useAddSeriesOptions } from 'AddSeries/addSeriesOptionsStore';
import { useSelect } from 'App/Select/SelectContext';
import CheckInput from 'Components/Form/CheckInput';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import SpinnerButton from 'Components/Link/SpinnerButton';
import PageContentFooter from 'Components/Page/PageContentFooter';
import Popover from 'Components/Tooltip/Popover';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import useSeries from 'Series/useSeries';
import { CheckInputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import ImportSeriesDefaults from './ImportSeriesDefaults';
import { getImportSeriesEligibility } from './importSeriesEligibility';
import {
  ImportSeriesItem,
  setImportSeriesViewOption,
  startProcessing,
  stopProcessing,
  updateImportSeriesItem,
  useImportSeriesItems,
  useImportSeriesViewOption,
  useLookupQueueHasItems,
} from './importSeriesStore';
import { useImportSeries } from './useImportSeries';
import styles from './ImportSeriesFooter.css';

function ImportSeriesFooter() {
  const defaults = useAddSeriesOptions();
  const items = useImportSeriesItems();
  const compactRows = useImportSeriesViewOption('compactRows');
  const isLookingUpSeries = useLookupQueueHasItems();
  const { data: existingSeries = [] } = useSeries();

  const existingTvdbIds = useMemo(() => {
    return new Set(existingSeries.map((series) => series.tvdbId));
  }, [existingSeries]);

  const { selectedCount, setItemStates, unselectAll, useSelectedIds } =
    useSelect<ImportSeriesItem>();
  const selectedIds = useSelectedIds();

  const { importSeries, isImporting, importError } = useImportSeries();

  const eligibility = useMemo(
    () => getImportSeriesEligibility(items, existingTvdbIds),
    [items, existingTvdbIds]
  );

  const itemById = useMemo(
    () => new Map(items.map((item) => [item.id, item])),
    [items]
  );

  const selectedReadyIds = useMemo(() => {
    const selectedTvdbIds = new Set<number>();

    return selectedIds.reduce<string[]>((readyIds, id) => {
      const item = itemById.get(id);
      const tvdbId = item?.selectedSeries?.tvdbId;
      const status = eligibility.eligibilityById.get(id);

      if (
        tvdbId != null &&
        (status === 'ready' || status === 'duplicate') &&
        !selectedTvdbIds.has(tvdbId)
      ) {
        selectedTvdbIds.add(tvdbId);
        readyIds.push(id);
      }

      return readyIds;
    }, []);
  }, [selectedIds, itemById, eligibility]);

  const selectedReadyIdSet = useMemo(
    () => new Set(selectedReadyIds),
    [selectedReadyIds]
  );

  const selectableCount = useMemo(() => {
    let count = eligibility.duplicateIdsByTvdbId.size;

    eligibility.eligibilityById.forEach((status) => {
      if (status === 'ready') {
        count++;
      }
    });

    return count;
  }, [eligibility]);

  const { hasUnsearchedItems, lookupCount, needsAttentionCount, readyCount } =
    useMemo(() => {
      let unsearchedCount = 0;
      let readyCount = 0;

      items.forEach((item) => {
        if (!item.hasSearched) {
          unsearchedCount++;
        }

        const status = eligibility.eligibilityById.get(item.id);

        if (
          status === 'ready' ||
          (status === 'duplicate' && selectedReadyIdSet.has(item.id))
        ) {
          readyCount++;
        }
      });

      return {
        hasUnsearchedItems: !isLookingUpSeries && unsearchedCount > 0,
        lookupCount: unsearchedCount,
        needsAttentionCount: items.length - readyCount,
        readyCount,
      };
    }, [items, isLookingUpSeries, eligibility, selectedReadyIdSet]);

  const selectAllValue = useMemo(() => {
    if (
      selectedReadyIds.length > 0 &&
      selectedReadyIds.length === selectableCount
    ) {
      return true;
    }

    if (selectedReadyIds.length === 0) {
      return false;
    }

    return null;
  }, [selectableCount, selectedReadyIds]);

  const handleSelectAllChange = useCallback(
    ({ value }: CheckInputChanged) => {
      if (value) {
        const selectedIdSet = new Set(selectedIds);
        const duplicateChoiceIds = new Set<string>();

        eligibility.duplicateIdsByTvdbId.forEach((ids) => {
          duplicateChoiceIds.add(
            ids.find((id) => selectedIdSet.has(id)) ?? ids[0]
          );
        });

        setItemStates(
          items.map((item) => {
            const status = eligibility.eligibilityById.get(item.id);

            return {
              id: item.id,
              isSelected: status === 'ready' || duplicateChoiceIds.has(item.id),
            };
          })
        );
      } else {
        unselectAll();
      }
    },
    [eligibility, items, selectedIds, setItemStates, unselectAll]
  );

  const handleCompactRowsChange = useCallback(
    ({ value }: CheckInputChanged) => {
      setImportSeriesViewOption('compactRows', value);
    },
    []
  );

  const handleLookupPress = useCallback(() => {
    startProcessing();
  }, []);

  const handleCancelLookupPress = useCallback(() => {
    stopProcessing();
  }, []);

  const handleImportPress = useCallback(() => {
    importSeries(selectedReadyIds);
  }, [importSeries, selectedReadyIds]);

  const handleApplyDefaults = useCallback(() => {
    selectedReadyIds.forEach((id) => {
      updateImportSeriesItem({
        id,
        monitor: defaults.monitor,
        qualityProfileId: defaults.qualityProfileId,
        seasonFolder: defaults.seasonFolder,
        seriesType: defaults.seriesType,
      });
    });
  }, [defaults, selectedReadyIds]);

  return (
    <PageContentFooter className={styles.footerShell}>
      <ImportSeriesDefaults
        isApplyDisabled={!selectedReadyIds.length}
        onApplyDefaults={handleApplyDefaults}
      />

      <div className={styles.footerContent}>
        <div className={styles.footerRow}>
          <div className={styles.selectionGroup}>
            <CheckInput
              className={styles.selectAllInput}
              containerClassName={styles.selectAllContainer}
              name="selectAllRows"
              ariaLabel={translate('SelectAll')}
              value={selectAllValue}
              onChange={handleSelectAllChange}
            />

            <span className={styles.selectionLabel}>
              {translate('CountOfTotalSelected', {
                selectedCount,
                totalCount: items.length,
              })}
            </span>
          </div>

          <div className={styles.queueStatus}>
            <span className={styles.statusItem}>
              <span className={styles.readyDot} />
              <strong>{translate('CountReady', { count: readyCount })}</strong>
            </span>

            {needsAttentionCount ? (
              <span className={styles.statusItem}>
                <span className={styles.attentionDot} />
                {translate('CountPending', { count: needsAttentionCount })}
              </span>
            ) : null}
          </div>

          <div className={styles.compactToggle}>
            <CheckInput
              className={styles.selectAllInput}
              containerClassName={styles.selectAllContainer}
              name="compactRows"
              ariaLabel={translate('CompactRows')}
              value={compactRows}
              onChange={handleCompactRowsChange}
            />

            <span className={styles.selectionLabel}>
              {translate('CompactRows')}
            </span>
          </div>

          <div className={styles.buttonGroup}>
            <div className={styles.importButtonContainer}>
              <SpinnerButton
                className={styles.importButton}
                kind={kinds.DEFAULT}
                isSpinning={isImporting}
                isDisabled={!selectedReadyIds.length || isLookingUpSeries}
                onPress={handleImportPress}
              >
                {translate('ImportCountSeries', {
                  selectedCount: selectedReadyIds.length,
                })}
              </SpinnerButton>

              {isLookingUpSeries ? (
                <Button
                  className={styles.stopButton}
                  kind={kinds.PRIMARY}
                  onPress={handleCancelLookupPress}
                >
                  <Icon name={icons.SPINNER} isSpinning={true} size={14} />

                  {translate('Stop')}
                </Button>
              ) : null}

              {hasUnsearchedItems ? (
                <Button
                  className={styles.loadingButton}
                  kind={kinds.PRIMARY}
                  onPress={handleLookupPress}
                >
                  {translate('SearchCountFolders', { count: lookupCount })}
                </Button>
              ) : null}

              {importError ? (
                <Popover
                  anchor={
                    <Icon
                      className={styles.importError}
                      name={icons.WARNING}
                      kind={kinds.WARNING}
                    />
                  }
                  title={translate('ImportErrors')}
                  body={
                    <ul>
                      {Array.isArray(importError.statusBody) ? (
                        importError.statusBody.map((error, index) => {
                          return <li key={index}>{error.errorMessage}</li>;
                        })
                      ) : (
                        <li>{JSON.stringify(importError.statusBody)}</li>
                      )}
                    </ul>
                  }
                  position={tooltipPositions.RIGHT}
                />
              ) : null}
            </div>
          </div>
        </div>
      </div>
    </PageContentFooter>
  );
}

export default ImportSeriesFooter;
