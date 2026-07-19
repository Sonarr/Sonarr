import React, { useCallback, useEffect, useMemo } from 'react';
import { useAddSeriesOptions } from 'AddSeries/addSeriesOptionsStore';
import { useSelect } from 'App/Select/SelectContext';
import CheckInput from 'Components/Form/CheckInput';
import MonitorEpisodesSelectInput from 'Components/Form/Select/MonitorEpisodesSelectInput';
import QualityProfileSelectInput from 'Components/Form/Select/QualityProfileSelectInput';
import SeriesTypeSelectInput from 'Components/Form/Select/SeriesTypeSelectInput';
import VirtualTableSelectCell from 'Components/Table/Cells/VirtualTableSelectCell';
import SeriesPoster from 'Series/SeriesPoster';
import useExistingSeries from 'Series/useExistingSeries';
import { CheckInputChanged, InputChanged } from 'typings/inputs';
import { SelectStateInputProps } from 'typings/props';
import translate from 'Utilities/String/translate';
import ImportSeriesChip from './ImportSeriesChip';
import {
  ImportSeriesItem,
  UnamppedFolderItem,
  updateImportSeriesItem,
  useImportSeriesItem,
} from './importSeriesStore';
import ImportSeriesSelectSeries from './SelectSeries/ImportSeriesSelectSeries';
import styles from './ImportSeriesRow.css';

interface ImportSeriesRowProps {
  unmappedFolder: UnamppedFolderItem;
}

function ImportSeriesRow({ unmappedFolder }: ImportSeriesRowProps) {
  const id = unmappedFolder.id;

  const item = useImportSeriesItem(unmappedFolder.id);

  const {
    relativePath,
    monitor,
    qualityProfileId,
    seasonFolder,
    seriesType,
    selectedSeries,
  } = item ?? {};

  const {
    monitor: defaultMonitor,
    qualityProfileId: defaultQualityProfileId,
    seriesType: defaultSeriesType,
  } = useAddSeriesOptions();

  const isExistingSeries = useExistingSeries(selectedSeries?.tvdbId);

  const { getIsSelected, toggleSelected, toggleDisabled } =
    useSelect<ImportSeriesItem>();

  const handleInputChange = useCallback(
    ({ name, value }: InputChanged) => {
      updateImportSeriesItem({ id, [name]: value });
    },
    [id]
  );

  const handleSeasonFolderChange = useCallback(
    ({ name, value }: CheckInputChanged) => {
      updateImportSeriesItem({ id, [name]: value });
    },
    [id]
  );

  const handleSelectedChange = useCallback(
    ({ id, value, shiftKey }: SelectStateInputProps<string>) => {
      toggleSelected({
        id,
        isSelected: value,
        shiftKey,
      });
    },
    [toggleSelected]
  );

  const isMonitorOverride = monitor !== undefined && monitor !== defaultMonitor;
  const isQualityProfileOverride =
    qualityProfileId !== undefined &&
    qualityProfileId !== defaultQualityProfileId;
  const isSeriesTypeOverride =
    seriesType !== undefined && seriesType !== defaultSeriesType;

  const monitorChipOptions = useMemo(
    () => ({
      label: translate('Monitor'),
      showDot: true,
      isOverride: isMonitorOverride,
    }),
    [isMonitorOverride]
  );
  const qualityChipOptions = useMemo(
    () => ({
      label: translate('QualityProfile'),
      isOverride: isQualityProfileOverride,
    }),
    [isQualityProfileOverride]
  );
  const seriesTypeChipOptions = useMemo(
    () => ({
      label: translate('SeriesType'),
      isOverride: isSeriesTypeOverride,
    }),
    [isSeriesTypeOverride]
  );

  useEffect(() => {
    toggleDisabled(id, !selectedSeries || isExistingSeries);
  }, [id, selectedSeries, isExistingSeries, toggleDisabled]);

  useEffect(() => {
    toggleSelected({ id, isSelected: !!selectedSeries, shiftKey: false });
  }, [id, selectedSeries, toggleSelected]);

  return (
    <div className={styles.rowLayout}>
      <div className={styles.metaStrip}>
        <VirtualTableSelectCell<string>
          inputClassName={styles.selectInput}
          id={id}
          isSelected={getIsSelected(id)}
          isDisabled={!selectedSeries || isExistingSeries}
          onSelectedChange={handleSelectedChange}
        />

        <div className={styles.folder}>{relativePath}</div>

        <div className={styles.seasonFolderGroup}>
          <span className={styles.seasonFolderLabel}>
            {translate('SeasonFolder')}
          </span>

          <CheckInput
            name="seasonFolder"
            value={seasonFolder}
            onChange={handleSeasonFolderChange}
          />
        </div>
      </div>

      <div className={styles.body}>
        <div className={styles.poster}>
          {selectedSeries ? (
            <SeriesPoster
              title={selectedSeries.title}
              images={selectedSeries.images}
              size={250}
              lazy={false}
              overflow={true}
            />
          ) : (
            <div className={styles.posterPlaceholder}>?</div>
          )}
        </div>

        <div className={styles.seriesInfo}>
          <div className={styles.titleRow}>
            <ImportSeriesSelectSeries
              id={id}
              onInputChange={handleInputChange}
            />
          </div>

          {selectedSeries ? (
            <div className={styles.chipStrip}>
              <MonitorEpisodesSelectInput
                className={styles.chipReset}
                name="monitor"
                value={monitor}
                selectedValueComponent={ImportSeriesChip}
                selectedValueOptions={monitorChipOptions}
                onChange={handleInputChange}
              />

              <QualityProfileSelectInput
                className={styles.chipReset}
                name="qualityProfileId"
                value={qualityProfileId}
                selectedValueComponent={ImportSeriesChip}
                selectedValueOptions={qualityChipOptions}
                onChange={handleInputChange}
              />

              <SeriesTypeSelectInput
                className={styles.chipReset}
                name="seriesType"
                value={seriesType}
                selectedValueComponent={ImportSeriesChip}
                selectedValueOptions={seriesTypeChipOptions}
                onChange={handleInputChange}
              />
            </div>
          ) : null}
        </div>
      </div>
    </div>
  );
}

export default ImportSeriesRow;
