import React, { useCallback, useMemo } from 'react';
import {
  AddSeriesOptions,
  setAddSeriesOption,
  useAddSeriesOptions,
} from 'AddSeries/addSeriesOptionsStore';
import EnhancedSelectInput, {
  EnhancedSelectInputValue,
} from 'Components/Form/Select/EnhancedSelectInput';
import MonitorEpisodesSelectInput from 'Components/Form/Select/MonitorEpisodesSelectInput';
import QualityProfileSelectInput from 'Components/Form/Select/QualityProfileSelectInput';
import SeriesTypeSelectInput from 'Components/Form/Select/SeriesTypeSelectInput';
import Button from 'Components/Link/Button';
import { kinds } from 'Helpers/Props';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import styles from './ImportSeriesDefaults.css';

const HIDE_HINT = { includeHint: false };

interface ImportSeriesDefaultsProps {
  isApplyDisabled: boolean;
  onApplyDefaults: () => void;
}

function ImportSeriesDefaults({
  isApplyDisabled,
  onApplyDefaults,
}: ImportSeriesDefaultsProps) {
  const { monitor, qualityProfileId, seasonFolder, seriesType } =
    useAddSeriesOptions();

  const seasonFolderOptions: EnhancedSelectInputValue<boolean>[] = useMemo(
    () => [
      { key: true, value: translate('Yes') },
      { key: false, value: translate('No') },
    ],
    []
  );

  const handleInputChange = useCallback(
    ({ name, value }: InputChanged<string | number | boolean | number[]>) => {
      setAddSeriesOption(name as keyof AddSeriesOptions, value);
    },
    []
  );

  return (
    <div className={styles.defaultsBar}>
      <div className={styles.defaultsContent}>
        <div className={styles.defaultsIdentity}>
          <div className={styles.defaultsTitle}>{translate('Defaults')}</div>
          <div className={styles.defaultsHint}>
            {translate('UsedForNewLookups')}
          </div>
        </div>

        <div className={styles.setting}>
          <div className={styles.settingLabel}>{translate('Monitor')}</div>
          <MonitorEpisodesSelectInput
            name="monitor"
            value={monitor}
            modalTitle={translate('Monitor')}
            onChange={handleInputChange}
          />
        </div>

        <div className={styles.setting}>
          <div className={styles.settingLabel}>
            {translate('QualityProfile')}
          </div>
          <QualityProfileSelectInput
            name="qualityProfileId"
            value={qualityProfileId}
            modalTitle={translate('QualityProfile')}
            onChange={handleInputChange}
          />
        </div>

        <div className={styles.setting}>
          <div className={styles.settingLabel}>{translate('SeriesType')}</div>
          <SeriesTypeSelectInput
            name="seriesType"
            value={seriesType}
            modalTitle={translate('SeriesType')}
            selectedValueOptions={HIDE_HINT}
            onChange={handleInputChange}
          />
        </div>

        <div className={styles.setting}>
          <div className={styles.settingLabel}>{translate('SeasonFolder')}</div>
          <EnhancedSelectInput
            name="seasonFolder"
            value={seasonFolder}
            modalTitle={translate('SeasonFolder')}
            values={seasonFolderOptions}
            onChange={handleInputChange}
          />
        </div>

        <Button
          className={styles.applyButton}
          kind={kinds.DEFAULT}
          isDisabled={isApplyDisabled}
          onPress={onApplyDefaults}
        >
          {translate('ApplyDefaultsToSelected')}
        </Button>
      </div>
    </div>
  );
}

export default ImportSeriesDefaults;
