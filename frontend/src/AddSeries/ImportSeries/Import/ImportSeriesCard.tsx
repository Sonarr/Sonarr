import classNames from 'classnames';
import React, { useCallback, useMemo } from 'react';
import { useAddSeriesOptions } from 'AddSeries/addSeriesOptionsStore';
import { useSelect } from 'App/Select/SelectContext';
import CheckInput from 'Components/Form/CheckInput';
import EnhancedSelectInput, {
  EnhancedSelectInputValue,
} from 'Components/Form/Select/EnhancedSelectInput';
import MonitorEpisodesSelectInput from 'Components/Form/Select/MonitorEpisodesSelectInput';
import QualityProfileSelectInput from 'Components/Form/Select/QualityProfileSelectInput';
import SeriesTypeSelectInput from 'Components/Form/Select/SeriesTypeSelectInput';
import Label from 'Components/Label';
import { kinds } from 'Helpers/Props';
import SeriesPoster from 'Series/SeriesPoster';
import useExistingSeries from 'Series/useExistingSeries';
import { CheckInputChanged, InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import {
  ImportSeriesItem,
  UnamppedFolderItem,
  updateImportSeriesItem,
  useImportSeriesItem,
} from './importSeriesStore';
import ImportSeriesSelectSeries from './SelectSeries/ImportSeriesSelectSeries';
import styles from './ImportSeriesCard.css';

interface ImportSeriesCardProps {
  unmappedFolder: UnamppedFolderItem;
  isCompact: boolean;
  isStacked: boolean;
}

function ImportSeriesCard({
  unmappedFolder,
  isCompact,
  isStacked,
}: ImportSeriesCardProps) {
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

  const { seasonFolder: defaultSeasonFolder } = useAddSeriesOptions();

  const isExistingSeries = useExistingSeries(selectedSeries?.tvdbId);
  const needsAttention = !selectedSeries || isExistingSeries;

  const { getIsSelected, toggleSelected } = useSelect<ImportSeriesItem>();

  const isSelected = getIsSelected(id);

  const handleInputChange = useCallback(
    ({ name, value }: InputChanged) => {
      updateImportSeriesItem({ id, [name]: value });
    },
    [id]
  );

  const handleSelectedChange = useCallback(
    ({ value, shiftKey }: CheckInputChanged) => {
      toggleSelected({
        id,
        isSelected: value,
        shiftKey,
      });
    },
    [id, toggleSelected]
  );

  const seasonFolderOptions: EnhancedSelectInputValue<boolean>[] = useMemo(
    () => [
      { key: true, value: translate('Yes') },
      { key: false, value: translate('No') },
    ],
    []
  );

  return (
    <div
      className={classNames(
        styles.rowLayout,
        isCompact && styles.compact,
        isStacked && styles.stacked,
        needsAttention && styles.needsAttention
      )}
    >
      <CheckInput
        className={styles.selectInput}
        containerClassName={styles.selectContainer}
        name={id}
        ariaLabel={translate('SelectRow')}
        value={isSelected}
        isDisabled={needsAttention}
        onChange={handleSelectedChange}
      />

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

      <div
        className={classNames(
          styles.seriesInfo,
          needsAttention && styles.attentionInfo
        )}
      >
        <div className={styles.identity}>
          {selectedSeries ? (
            <>
              <div className={styles.titleRow}>
                <ImportSeriesSelectSeries
                  id={id}
                  onInputChange={handleInputChange}
                />
              </div>

              <div className={styles.source}>
                <span className={styles.folder}>{relativePath}</span>
                {selectedSeries.network ? ` · ${selectedSeries.network}` : null}
              </div>
            </>
          ) : (
            <>
              <div className={styles.folderName}>{relativePath}</div>
              <div className={styles.source}>
                <span className={styles.folder}>{unmappedFolder.path}</span>
              </div>
            </>
          )}
        </div>

        {needsAttention ? (
          <>
            <Label
              className={classNames(
                styles.matchStatus,
                !selectedSeries && styles.matchStatusStackedHidden
              )}
              kind={kinds.WARNING}
            >
              {translate(isExistingSeries ? 'AlreadyInLibrary' : 'Pending')}
            </Label>

            {selectedSeries ? null : (
              <div className={styles.matchAction}>
                <ImportSeriesSelectSeries
                  id={id}
                  onInputChange={handleInputChange}
                />
              </div>
            )}
          </>
        ) : (
          <div className={styles.settingsRow}>
            <div className={styles.setting}>
              <div className={styles.settingLabel}>{translate('Monitor')}</div>

              <MonitorEpisodesSelectInput
                name="monitor"
                value={monitor}
                isDisabled={!selectedSeries}
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
                isDisabled={!selectedSeries}
                onChange={handleInputChange}
              />
            </div>

            <div className={styles.setting}>
              <div className={styles.settingLabel}>
                {translate('SeriesType')}
              </div>

              <SeriesTypeSelectInput
                name="seriesType"
                value={seriesType}
                isDisabled={!selectedSeries}
                onChange={handleInputChange}
              />
            </div>

            <div className={styles.setting}>
              <div className={styles.settingLabel}>
                {translate('SeasonFolder')}
              </div>

              <EnhancedSelectInput
                name="seasonFolder"
                value={seasonFolder ?? defaultSeasonFolder}
                values={seasonFolderOptions}
                isDisabled={!selectedSeries}
                onChange={handleInputChange}
              />
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

export default ImportSeriesCard;
