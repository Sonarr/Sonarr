import React, { useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import { useSelect } from 'App/SelectContext';
import AppState from 'App/State/AppState';
import { ImportSeries } from 'App/State/ImportSeriesAppState';
import FormInputGroup from 'Components/Form/FormInputGroup';
import VirtualTableRowCell from 'Components/Table/Cells/VirtualTableRowCell';
import VirtualTableSelectCell from 'Components/Table/Cells/VirtualTableSelectCell';
import { inputTypes } from 'Helpers/Props';
import { setImportSeriesValue } from 'Store/Actions/importSeriesActions';
import createExistingSeriesSelector from 'Store/Selectors/createExistingSeriesSelector';
import { InputChanged } from 'typings/inputs';
import { SelectStateInputProps } from 'typings/props';
import ImportSeriesSelectSeries from './SelectSeries/ImportSeriesSelectSeries';
import styles from './ImportSeriesRow.css';

function createItemSelector(id: string) {
  return createSelector(
    (state: AppState) => state.importSeries.items,
    (items) => {
      return (
        items.find((item) => {
          return item.id === id;
        }) || ({} as ImportSeries)
      );
    }
  );
}

interface ImportSeriesRowProps {
  id: string;
}

function ImportSeriesRow({ id }: ImportSeriesRowProps) {
  const dispatch = useDispatch();

  const {
    relativePath,
    monitor,
    qualityProfileId,
    seasonFolder,
    seriesType,
    selectedSeries,
  } = useSelector(createItemSelector(id));

  const isExistingSeries = useSelector(
    createExistingSeriesSelector(selectedSeries?.tvdbId)
  );

  const [selectState, selectDispatch] = useSelect();

  const handleInputChange = useCallback(
    ({ name, value }: InputChanged) => {
      dispatch(
        // @ts-expect-error - actions are not typed
        setImportSeriesValue({
          id,
          [name]: value,
        })
      );
    },
    [id, dispatch]
  );

  const handleSelectedChange = useCallback(
    ({ id, value, shiftKey }: SelectStateInputProps) => {
      selectDispatch({
        type: 'toggleSelected',
        id,
        isSelected: value,
        shiftKey,
      });
    },
    [selectDispatch]
  );

  console.info(
    '\x1b[36m[MarkTest] is selected\x1b[0m',
    selectState.selectedState[id]
  );

  return (
    <>
      <VirtualTableSelectCell
        inputClassName={styles.selectInput}
        id={id}
        isSelected={selectState.selectedState[id]}
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
