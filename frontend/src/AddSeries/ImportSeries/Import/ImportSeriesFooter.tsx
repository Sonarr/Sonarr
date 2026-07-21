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
import {
  ImportSeriesItem,
  isReadyToImport,
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

  const {
    selectedCount,
    getSelectedIds,
    allSelected,
    allUnselected,
    selectAll,
    unselectAll,
  } = useSelect<ImportSeriesItem>();

  const { importSeries, isImporting, importError } = useImportSeries();

  const { hasUnsearchedItems, lookupCount, needsAttentionCount, readyCount } =
    useMemo(() => {
      let unsearchedCount = 0;
      let matchedCount = 0;

      items.forEach((item) => {
        if (!item.hasSearched) {
          unsearchedCount++;
        }

        if (isReadyToImport(item, existingTvdbIds)) {
          matchedCount++;
        }
      });

      return {
        hasUnsearchedItems: !isLookingUpSeries && unsearchedCount > 0,
        lookupCount: unsearchedCount,
        needsAttentionCount: items.length - matchedCount,
        readyCount: matchedCount,
      };
    }, [existingTvdbIds, items, isLookingUpSeries]);

  const selectAllValue = useMemo(() => {
    if (allSelected) {
      return true;
    }

    if (allUnselected) {
      return false;
    }

    return null;
  }, [allSelected, allUnselected]);

  const handleSelectAllChange = useCallback(
    ({ value }: CheckInputChanged) => {
      if (value) {
        selectAll();
      } else {
        unselectAll();
      }
    },
    [selectAll, unselectAll]
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
    importSeries(getSelectedIds());
  }, [importSeries, getSelectedIds]);

  const handleApplyDefaults = useCallback(() => {
    getSelectedIds().forEach((id) => {
      updateImportSeriesItem({
        id,
        monitor: defaults.monitor,
        qualityProfileId: defaults.qualityProfileId,
        seasonFolder: defaults.seasonFolder,
        seriesType: defaults.seriesType,
      });
    });
  }, [defaults, getSelectedIds]);

  return (
    <PageContentFooter className={styles.footerShell}>
      <ImportSeriesDefaults
        isApplyDisabled={!selectedCount}
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
                {translate('CountNeedsAMatch', { count: needsAttentionCount })}
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
                isDisabled={!selectedCount || isLookingUpSeries}
                onPress={handleImportPress}
              >
                {translate('ImportCountSeries', {
                  selectedCount,
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
                  {translate('ResolveUnmatched', { count: lookupCount })}
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
