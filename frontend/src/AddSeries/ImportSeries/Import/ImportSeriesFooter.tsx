import React, { useCallback, useEffect, useMemo, useState } from 'react';
import {
  AddSeriesOptions,
  setAddSeriesOption,
  useAddSeriesOptions,
} from 'AddSeries/addSeriesOptionsStore';
import { useSelect } from 'App/Select/SelectContext';
import CheckInput from 'Components/Form/CheckInput';
import FormInputGroup from 'Components/Form/FormInputGroup';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import SpinnerButton from 'Components/Link/SpinnerButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContentFooter from 'Components/Page/PageContentFooter';
import Popover from 'Components/Tooltip/Popover';
import { icons, inputTypes, kinds, tooltipPositions } from 'Helpers/Props';
import { SeriesMonitor, SeriesType } from 'Series/Series';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import {
  ImportSeriesItem,
  startProcessing,
  stopProcessing,
  updateImportSeriesItem,
  useImportSeriesItems,
  useLookupQueueHasItems,
} from './importSeriesStore';
import { useImportSeries } from './useImportSeries';
import styles from './ImportSeriesFooter.css';

type MixedType = 'mixed';

function ImportSeriesFooter() {
  const {
    monitor: defaultMonitor,
    qualityProfileId: defaultQualityProfileId,
    seriesType: defaultSeriesType,
    seasonFolder: defaultSeasonFolder,
  } = useAddSeriesOptions();

  const items = useImportSeriesItems();
  const isLookingUpSeries = useLookupQueueHasItems();

  const [monitor, setMonitor] = useState<SeriesMonitor | MixedType>(
    defaultMonitor
  );
  const [qualityProfileId, setQualityProfileId] = useState<number | MixedType>(
    defaultQualityProfileId
  );
  const [seriesType, setSeriesType] = useState<SeriesType | MixedType>(
    defaultSeriesType
  );
  const [seasonFolder, setSeasonFolder] = useState<boolean | MixedType>(
    defaultSeasonFolder
  );

  const { selectedCount, getSelectedIds } = useSelect<ImportSeriesItem>();

  const { importSeries, isImporting, importError } = useImportSeries();

  const {
    hasUnsearchedItems,
    isMonitorMixed,
    isQualityProfileIdMixed,
    isSeriesTypeMixed,
    isSeasonFolderMixed,
  } = useMemo(() => {
    let isMonitorMixed = false;
    let isQualityProfileIdMixed = false;
    let isSeriesTypeMixed = false;
    let isSeasonFolderMixed = false;
    let hasUnsearchedItems = false;

    items.forEach((item) => {
      if (item.monitor !== defaultMonitor) {
        isMonitorMixed = true;
      }

      if (item.qualityProfileId !== defaultQualityProfileId) {
        isQualityProfileIdMixed = true;
      }

      if (item.seriesType !== defaultSeriesType) {
        isSeriesTypeMixed = true;
      }

      if (item.seasonFolder !== defaultSeasonFolder) {
        isSeasonFolderMixed = true;
      }

      if (!item.hasSearched) {
        hasUnsearchedItems = true;
      }
    });

    return {
      hasUnsearchedItems: !isLookingUpSeries && hasUnsearchedItems,
      isMonitorMixed,
      isQualityProfileIdMixed,
      isSeriesTypeMixed,
      isSeasonFolderMixed,
    };
  }, [
    defaultMonitor,
    defaultQualityProfileId,
    defaultSeasonFolder,
    defaultSeriesType,
    items,
    isLookingUpSeries,
  ]);

  const handleInputChange = useCallback(
    ({ name, value }: InputChanged<string | number | boolean | number[]>) => {
      if (name === 'monitor') {
        setMonitor(value as SeriesMonitor);
      } else if (name === 'qualityProfileId') {
        setQualityProfileId(value as number);
      } else if (name === 'seriesType') {
        setSeriesType(value as SeriesType);
      } else if (name === 'seasonFolder') {
        setSeasonFolder(value as boolean);
      }

      setAddSeriesOption(name as keyof AddSeriesOptions, value);

      getSelectedIds().forEach((id) => {
        updateImportSeriesItem({
          id,
          [name]: value,
        });
      });
    },
    [getSelectedIds]
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

  useEffect(() => {
    if (isMonitorMixed && monitor !== 'mixed') {
      setMonitor('mixed');
    } else if (!isMonitorMixed && monitor !== defaultMonitor) {
      setMonitor(defaultMonitor);
    }
  }, [defaultMonitor, isMonitorMixed, monitor]);

  useEffect(() => {
    if (isQualityProfileIdMixed && qualityProfileId !== 'mixed') {
      setQualityProfileId('mixed');
    } else if (
      !isQualityProfileIdMixed &&
      qualityProfileId !== defaultQualityProfileId
    ) {
      setQualityProfileId(defaultQualityProfileId);
    }
  }, [defaultQualityProfileId, isQualityProfileIdMixed, qualityProfileId]);

  useEffect(() => {
    if (isSeriesTypeMixed && seriesType !== 'mixed') {
      setSeriesType('mixed');
    } else if (!isSeriesTypeMixed && seriesType !== defaultSeriesType) {
      setSeriesType(defaultSeriesType);
    }
  }, [defaultSeriesType, isSeriesTypeMixed, seriesType]);

  useEffect(() => {
    if (isSeasonFolderMixed && seasonFolder !== 'mixed') {
      setSeasonFolder('mixed');
    } else if (!isSeasonFolderMixed && seasonFolder !== defaultSeasonFolder) {
      setSeasonFolder(defaultSeasonFolder);
    }
  }, [defaultSeasonFolder, isSeasonFolderMixed, seasonFolder]);

  return (
    <PageContentFooter>
      <div className={styles.inputContainer}>
        <div className={styles.label}>{translate('Monitor')}</div>

        <FormInputGroup
          type={inputTypes.MONITOR_EPISODES_SELECT}
          name="monitor"
          value={monitor}
          isDisabled={!selectedCount}
          includeMixed={isMonitorMixed}
          onChange={handleInputChange}
        />
      </div>

      <div className={styles.inputContainer}>
        <div className={styles.label}>{translate('QualityProfile')}</div>

        <FormInputGroup
          type={inputTypes.QUALITY_PROFILE_SELECT}
          name="qualityProfileId"
          value={qualityProfileId}
          isDisabled={!selectedCount}
          includeMixed={isQualityProfileIdMixed}
          onChange={handleInputChange}
        />
      </div>

      <div className={styles.inputContainer}>
        <div className={styles.label}>{translate('SeriesType')}</div>

        <FormInputGroup
          type={inputTypes.SERIES_TYPE_SELECT}
          name="seriesType"
          value={seriesType}
          isDisabled={!selectedCount}
          includeMixed={isSeriesTypeMixed}
          onChange={handleInputChange}
        />
      </div>

      <div className={styles.inputContainer}>
        <div className={styles.label}>{translate('SeasonFolder')}</div>

        <CheckInput
          name="seasonFolder"
          value={seasonFolder}
          isDisabled={!selectedCount}
          onChange={handleInputChange}
        />
      </div>

      <div>
        <div className={styles.label}>&nbsp;</div>

        <div className={styles.importButtonContainer}>
          <SpinnerButton
            className={styles.importButton}
            kind={kinds.PRIMARY}
            isSpinning={isImporting}
            isDisabled={!selectedCount || isLookingUpSeries}
            onPress={handleImportPress}
          >
            {translate('ImportCountSeries', { selectedCount })}
          </SpinnerButton>

          {isLookingUpSeries ? (
            <Button
              className={styles.loadingButton}
              kind={kinds.WARNING}
              onPress={handleCancelLookupPress}
            >
              {translate('CancelProcessing')}
            </Button>
          ) : null}

          {hasUnsearchedItems ? (
            <Button
              className={styles.loadingButton}
              kind={kinds.SUCCESS}
              onPress={handleLookupPress}
            >
              {translate('StartProcessing')}
            </Button>
          ) : null}

          {isLookingUpSeries ? (
            <LoadingIndicator className={styles.loading} size={24} />
          ) : null}

          {isLookingUpSeries ? translate('ProcessingFolders') : null}

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
    </PageContentFooter>
  );
}

export default ImportSeriesFooter;
