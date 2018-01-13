import PropTypes from 'prop-types';
import React from 'react';
import { inputTypes } from 'Helpers/Props';
import FormInputGroup from 'Components/Form/FormInputGroup';
import VirtualTableRow from 'Components/Table/VirtualTableRow';
import VirtualTableRowCell from 'Components/Table/Cells/VirtualTableRowCell';
import VirtualTableSelectCell from 'Components/Table/Cells/VirtualTableSelectCell';
import ImportSeriesSelectSeriesConnector from './SelectSeries/ImportSeriesSelectSeriesConnector';
import styles from './ImportSeriesRow.css';

function ImportSeriesRow(props) {
  const {
    style,
    id,
    monitor,
    qualityProfileId,
    languageProfileId,
    seasonFolder,
    seriesType,
    selectedSeries,
    isExistingSeries,
    showLanguageProfile,
    isSelected,
    onSelectedChange,
    onInputChange
  } = props;

  return (
    <VirtualTableRow style={style}>
      <VirtualTableSelectCell
        inputClassName={styles.selectInput}
        id={id}
        isSelected={isSelected}
        isDisabled={!selectedSeries || isExistingSeries}
        onSelectedChange={onSelectedChange}
      />

      <VirtualTableRowCell className={styles.folder}>
        {id}
      </VirtualTableRowCell>

      <VirtualTableRowCell className={styles.monitor}>
        <FormInputGroup
          type={inputTypes.MONITOR_EPISODES_SELECT}
          name="monitor"
          value={monitor}
          onChange={onInputChange}
        />
      </VirtualTableRowCell>

      <VirtualTableRowCell className={styles.qualityProfile}>
        <FormInputGroup
          type={inputTypes.QUALITY_PROFILE_SELECT}
          name="qualityProfileId"
          value={qualityProfileId}
          onChange={onInputChange}
        />
      </VirtualTableRowCell>

      <VirtualTableRowCell
        className={showLanguageProfile ? styles.languageProfile : styles.hideLanguageProfile}
      >
        <FormInputGroup
          type={inputTypes.LANGUAGE_PROFILE_SELECT}
          name="languageProfileId"
          value={languageProfileId}
          onChange={onInputChange}
        />
      </VirtualTableRowCell>

      <VirtualTableRowCell className={styles.seriesType}>
        <FormInputGroup
          type={inputTypes.SERIES_TYPE_SELECT}
          name="seriesType"
          value={seriesType}
          onChange={onInputChange}
        />
      </VirtualTableRowCell>

      <VirtualTableRowCell className={styles.seasonFolder}>
        <FormInputGroup
          type={inputTypes.CHECK}
          name="seasonFolder"
          value={seasonFolder}
          onChange={onInputChange}
        />
      </VirtualTableRowCell>

      <VirtualTableRowCell className={styles.series}>
        <ImportSeriesSelectSeriesConnector
          id={id}
          isExistingSeries={isExistingSeries}
        />
      </VirtualTableRowCell>
    </VirtualTableRow>
  );
}

ImportSeriesRow.propTypes = {
  style: PropTypes.object.isRequired,
  id: PropTypes.string.isRequired,
  monitor: PropTypes.string.isRequired,
  qualityProfileId: PropTypes.number.isRequired,
  languageProfileId: PropTypes.number.isRequired,
  seriesType: PropTypes.string.isRequired,
  seasonFolder: PropTypes.bool.isRequired,
  selectedSeries: PropTypes.object,
  isExistingSeries: PropTypes.bool.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  queued: PropTypes.bool.isRequired,
  showLanguageProfile: PropTypes.bool.isRequired,
  isSelected: PropTypes.bool,
  onSelectedChange: PropTypes.func.isRequired,
  onInputChange: PropTypes.func.isRequired
};

ImportSeriesRow.defaultsProps = {
  items: []
};

export default ImportSeriesRow;
