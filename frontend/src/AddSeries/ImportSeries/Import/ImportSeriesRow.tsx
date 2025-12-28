import React, { useCallback, useEffect } from 'react';
import { useSelect } from 'App/Select/SelectContext';
import FormInputGroup from 'Components/Form/FormInputGroup';
import VirtualTableRowCell from 'Components/Table/Cells/VirtualTableRowCell';
import VirtualTableSelectCell from 'Components/Table/Cells/VirtualTableSelectCell';
import { inputTypes } from 'Helpers/Props';
import useExistingSeries from 'Series/useExistingSeries';
import { InputChanged } from 'typings/inputs';
import { SelectStateInputProps } from 'typings/props';
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

  const isExistingSeries = useExistingSeries(selectedSeries?.tvdbId);

  const { getIsSelected, toggleSelected, toggleDisabled } =
    useSelect<ImportSeriesItem>();

  const handleInputChange = useCallback(
    ({ name, value }: InputChanged) => {
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

  useEffect(() => {
    toggleDisabled(id, !selectedSeries || isExistingSeries);
  }, [id, selectedSeries, isExistingSeries, toggleDisabled]);

  useEffect(() => {
    toggleSelected({ id, isSelected: !!selectedSeries, shiftKey: false });
  }, [id, selectedSeries, toggleSelected]);

  return (
    <>
      <VirtualTableSelectCell<string>
        inputClassName={styles.selectInput}
        id={id}
        isSelected={getIsSelected(id)}
        isDisabled={!selectedSeries || isExistingSeries}
        onSelectedChange={handleSelectedChange}
      />

      <VirtualTableRowCell className={styles.folder}>
        {relativePath}
      </VirtualTableRowCell>

      <VirtualTableRowCell className={styles.monitor}>
        <FormInputGroup
          type={inputTypes.MONITOR_EPISODES_SELECT}
          name="monitor"
          value={monitor}
          onChange={handleInputChange}
        />
      </VirtualTableRowCell>

      <VirtualTableRowCell className={styles.qualityProfile}>
        <FormInputGroup
          type={inputTypes.QUALITY_PROFILE_SELECT}
          name="qualityProfileId"
          value={qualityProfileId}
          onChange={handleInputChange}
        />
      </VirtualTableRowCell>

      <VirtualTableRowCell className={styles.seriesType}>
        <FormInputGroup
          type={inputTypes.SERIES_TYPE_SELECT}
          name="seriesType"
          value={seriesType}
          onChange={handleInputChange}
        />
      </VirtualTableRowCell>

      <VirtualTableRowCell className={styles.seasonFolder}>
        <FormInputGroup
          type={inputTypes.CHECK}
          name="seasonFolder"
          value={seasonFolder}
          onChange={handleInputChange}
        />
      </VirtualTableRowCell>

      <VirtualTableRowCell className={styles.series}>
        <ImportSeriesSelectSeries id={id} onInputChange={handleInputChange} />
      </VirtualTableRowCell>
    </>
  );
}

export default ImportSeriesRow;
