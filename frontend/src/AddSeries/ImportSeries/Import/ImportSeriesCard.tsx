import classNames from 'classnames';
import React, { useCallback, useMemo, useState } from 'react';
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
import { CheckInputChanged, InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import { ImportSeriesEligibility } from './importSeriesEligibility';
import {
  ImportSeriesItem,
  UnamppedFolderItem,
  updateImportSeriesItem,
  useImportSeriesItem,
} from './importSeriesStore';
import ImportSeriesSelectSeries from './SelectSeries/ImportSeriesSelectSeries';
import styles from './ImportSeriesCard.css';

const HIDE_HINT = { includeHint: false };

function getMatchStatusLabel(eligibility: ImportSeriesEligibility) {
  switch (eligibility) {
    case 'existing':
      return translate('AlreadyInLibrary');
    case 'duplicate':
      return translate('DuplicateMatch');
    case 'unmatched':
      return translate('Pending');
    case 'ready':
    default:
      return null;
  }
}

interface ImportSeriesCardProps {
  unmappedFolder: UnamppedFolderItem;
  duplicateIds: string[];
  eligibility: ImportSeriesEligibility;
  isCompact: boolean;
  isStacked: boolean;
}

function ImportSeriesCard({
  unmappedFolder,
  duplicateIds,
  eligibility,
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

  const { setItemStates, toggleSelected, useIsSelected } =
    useSelect<ImportSeriesItem>();
  const isSelected = useIsSelected(id);
  const isBlocked = eligibility === 'unmatched' || eligibility === 'existing';
  const needsAttention =
    isBlocked || (eligibility === 'duplicate' && !isSelected);
  const matchStatusLabel = getMatchStatusLabel(eligibility);

  const handleInputChange = useCallback(
    ({ name, value }: InputChanged) => {
      updateImportSeriesItem({ id, [name]: value });
    },
    [id]
  );

  const handleSelectedChange = useCallback(
    ({ value, shiftKey }: CheckInputChanged) => {
      if (eligibility === 'duplicate') {
        setItemStates(
          value
            ? duplicateIds.map((duplicateId) => ({
                id: duplicateId,
                isSelected: duplicateId === id,
              }))
            : [{ id, isSelected: false }]
        );

        return;
      }

      toggleSelected({
        id,
        isSelected: value,
        shiftKey,
      });
    },
    [id, duplicateIds, eligibility, setItemStates, toggleSelected]
  );

  const [isSearching, setIsSearching] = useState(false);

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
        needsAttention && styles.needsAttention,
        isSearching && styles.searching
      )}
    >
      <CheckInput
        className={styles.selectInput}
        containerClassName={styles.selectContainer}
        name={id}
        ariaLabel={translate('SelectRow')}
        value={isSelected}
        isDisabled={isBlocked}
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
                  onEditingChange={setIsSearching}
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

        {needsAttention && matchStatusLabel ? (
          <>
            <Label
              className={classNames(
                styles.matchStatus,
                !selectedSeries && styles.matchStatusStackedHidden
              )}
              kind={kinds.WARNING}
            >
              {matchStatusLabel}
            </Label>

            {selectedSeries ? null : (
              <div className={styles.matchAction}>
                <ImportSeriesSelectSeries
                  id={id}
                  onInputChange={handleInputChange}
                  onEditingChange={setIsSearching}
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
                modalTitle={translate('Monitor')}
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
                modalTitle={translate('QualityProfile')}
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
                modalTitle={translate('SeriesType')}
                isDisabled={!selectedSeries}
                selectedValueOptions={HIDE_HINT}
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
                modalTitle={translate('SeasonFolder')}
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
