import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { useSelect } from 'App/Select/SelectContext';
import Icon from 'Components/Icon';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRowCellButton from 'Components/Table/Cells/TableRowCellButton';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import Column from 'Components/Table/Column';
import TableRow from 'Components/Table/TableRow';
import Popover from 'Components/Tooltip/Popover';
import Episode from 'Episode/Episode';
import EpisodeFormats from 'Episode/EpisodeFormats';
import EpisodeLanguages from 'Episode/EpisodeLanguages';
import EpisodeQuality from 'Episode/EpisodeQuality';
import getReleaseTypeName from 'Episode/getReleaseTypeName';
import IndexerFlags from 'Episode/IndexerFlags';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import SelectEpisodeModal from 'InteractiveImport/Episode/SelectEpisodeModal';
import { SelectedEpisode } from 'InteractiveImport/Episode/SelectEpisodeModalContent';
import SelectIndexerFlagsModal from 'InteractiveImport/IndexerFlags/SelectIndexerFlagsModal';
import InteractiveImport from 'InteractiveImport/InteractiveImport';
import SelectLanguageModal from 'InteractiveImport/Language/SelectLanguageModal';
import SelectQualityModal from 'InteractiveImport/Quality/SelectQualityModal';
import SelectReleaseGroupModal from 'InteractiveImport/ReleaseGroup/SelectReleaseGroupModal';
import ReleaseType from 'InteractiveImport/ReleaseType';
import SelectReleaseTypeModal from 'InteractiveImport/ReleaseType/SelectReleaseTypeModal';
import SelectSeasonModal from 'InteractiveImport/Season/SelectSeasonModal';
import SelectSeriesModal from 'InteractiveImport/Series/SelectSeriesModal';
import { useUpdateInteractiveImportItem } from 'InteractiveImport/useInteractiveImport';
import Language from 'Language/Language';
import { QualityModel } from 'Quality/Quality';
import Series from 'Series/Series';
import CustomFormat from 'typings/CustomFormat';
import { SelectStateInputProps } from 'typings/props';
import Rejection from 'typings/Rejection';
import formatBytes from 'Utilities/Number/formatBytes';
import formatCustomFormatScore from 'Utilities/Number/formatCustomFormatScore';
import translate from 'Utilities/String/translate';
import InteractiveImportRowCellPlaceholder from './InteractiveImportRowCellPlaceholder';
import styles from './InteractiveImportRow.css';

type SelectType =
  | 'series'
  | 'season'
  | 'episode'
  | 'releaseGroup'
  | 'quality'
  | 'language'
  | 'indexerFlags'
  | 'releaseType';

type SelectedChangeProps = SelectStateInputProps & {
  hasEpisodeFileId: boolean;
};

interface InteractiveImportRowProps {
  id: number;
  allowSeriesChange: boolean;
  relativePath: string;
  series?: Series;
  seasonNumber?: number;
  episodes?: Episode[];
  releaseGroup?: string;
  quality?: QualityModel;
  languages?: Language[];
  size: number;
  releaseType: ReleaseType;
  customFormats?: CustomFormat[];
  customFormatScore?: number;
  indexerFlags: number;
  rejections: Rejection[];
  columns: Column[];
  episodeFileId?: number;
  isReprocessing?: boolean;
  modalTitle: string;
  onReprocessItems: (ids: number[]) => void;
  onSelectedChange(result: SelectedChangeProps): void;
  onValidRowChange(id: number, isValid: boolean): void;
}

function InteractiveImportRow(props: InteractiveImportRowProps) {
  const {
    id,
    allowSeriesChange,
    relativePath,
    series,
    seasonNumber,
    episodes = [],
    quality,
    languages,
    releaseGroup,
    size,
    releaseType,
    customFormats = [],
    customFormatScore,
    indexerFlags,
    rejections,
    isReprocessing,
    modalTitle,
    episodeFileId,
    columns,
    onReprocessItems,
    onSelectedChange,
    onValidRowChange,
  } = props;

  const { useIsSelected } = useSelect<InteractiveImport>();
  const isSelected = useIsSelected(id);
  const { updateInteractiveImportItem } = useUpdateInteractiveImportItem();

  const isSeriesColumnVisible = useMemo(
    () => columns.find((c) => c.name === 'series')?.isVisible ?? false,
    [columns]
  );
  const isIndexerFlagsColumnVisible = useMemo(
    () => columns.find((c) => c.name === 'indexerFlags')?.isVisible ?? false,
    [columns]
  );

  const [selectModalOpen, setSelectModalOpen] = useState<SelectType | null>(
    null
  );

  useEffect(
    () => {
      if (
        allowSeriesChange &&
        series &&
        seasonNumber != null &&
        episodes.length &&
        quality &&
        languages &&
        size > 0
      ) {
        onSelectedChange({
          id,
          hasEpisodeFileId: !!episodeFileId,
          value: true,
          shiftKey: false,
        });
      }
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    []
  );

  useEffect(() => {
    const isValid = !!(
      series &&
      seasonNumber != null &&
      episodes.length &&
      quality &&
      languages
    );

    if (isSelected && !isValid) {
      onValidRowChange(id, false);
    } else {
      onValidRowChange(id, true);
    }
  }, [
    id,
    series,
    seasonNumber,
    episodes,
    quality,
    languages,
    isSelected,
    onValidRowChange,
  ]);

  const handleSelectedChange = useCallback(
    (result: SelectStateInputProps) => {
      onSelectedChange({
        ...result,
        hasEpisodeFileId: !!episodeFileId,
      });
    },
    [episodeFileId, onSelectedChange]
  );

  const selectRowAfterChange = useCallback(() => {
    if (!isSelected) {
      onSelectedChange({
        id,
        hasEpisodeFileId: !!episodeFileId,
        value: true,
        shiftKey: false,
      });
    }
  }, [id, episodeFileId, isSelected, onSelectedChange]);

  const onSelectModalClose = useCallback(() => {
    setSelectModalOpen(null);
  }, [setSelectModalOpen]);

  const onSelectSeriesPress = useCallback(() => {
    setSelectModalOpen('series');
  }, [setSelectModalOpen]);

  const onSeriesSelect = useCallback(
    (series: Series) => {
      updateInteractiveImportItem(id, {
        series,
        seasonNumber: undefined,
        episodes: [],
      });

      onReprocessItems([id]);
      setSelectModalOpen(null);
      selectRowAfterChange();
    },
    [
      id,
      updateInteractiveImportItem,
      onReprocessItems,
      setSelectModalOpen,
      selectRowAfterChange,
    ]
  );

  const onSelectSeasonPress = useCallback(() => {
    setSelectModalOpen('season');
  }, [setSelectModalOpen]);

  const onSeasonSelect = useCallback(
    (seasonNumber: number) => {
      updateInteractiveImportItem(id, {
        seasonNumber,
        episodes: [],
      });

      onReprocessItems([id]);
      setSelectModalOpen(null);
      selectRowAfterChange();
    },
    [
      id,
      updateInteractiveImportItem,
      onReprocessItems,
      setSelectModalOpen,
      selectRowAfterChange,
    ]
  );

  const onSelectEpisodePress = useCallback(() => {
    setSelectModalOpen('episode');
  }, [setSelectModalOpen]);

  const onEpisodesSelect = useCallback(
    (selectedEpisodes: SelectedEpisode[]) => {
      const episodes = selectedEpisodes[0].episodes;
      updateInteractiveImportItem(id, { episodes });
      onReprocessItems([id]);

      setSelectModalOpen(null);
      selectRowAfterChange();
    },
    [
      id,
      updateInteractiveImportItem,
      onReprocessItems,
      setSelectModalOpen,
      selectRowAfterChange,
    ]
  );

  const onSelectReleaseGroupPress = useCallback(() => {
    setSelectModalOpen('releaseGroup');
  }, [setSelectModalOpen]);

  const onReleaseGroupSelect = useCallback(
    (releaseGroup: string) => {
      updateInteractiveImportItem(id, { releaseGroup });
      onReprocessItems([id]);

      setSelectModalOpen(null);
      selectRowAfterChange();
    },
    [
      id,
      updateInteractiveImportItem,
      onReprocessItems,
      setSelectModalOpen,
      selectRowAfterChange,
    ]
  );

  const onSelectQualityPress = useCallback(() => {
    setSelectModalOpen('quality');
  }, [setSelectModalOpen]);

  const onQualitySelect = useCallback(
    (quality: QualityModel) => {
      updateInteractiveImportItem(id, { quality });
      onReprocessItems([id]);

      setSelectModalOpen(null);
      selectRowAfterChange();
    },
    [
      id,
      updateInteractiveImportItem,
      onReprocessItems,
      setSelectModalOpen,
      selectRowAfterChange,
    ]
  );

  const onSelectLanguagePress = useCallback(() => {
    setSelectModalOpen('language');
  }, [setSelectModalOpen]);

  const onLanguagesSelect = useCallback(
    (languages: Language[]) => {
      updateInteractiveImportItem(id, { languages });
      onReprocessItems([id]);

      setSelectModalOpen(null);
      selectRowAfterChange();
    },
    [
      id,
      updateInteractiveImportItem,
      onReprocessItems,
      setSelectModalOpen,
      selectRowAfterChange,
    ]
  );

  const onSelectReleaseTypePress = useCallback(() => {
    setSelectModalOpen('releaseType');
  }, [setSelectModalOpen]);

  const onReleaseTypeSelect = useCallback(
    (releaseType: ReleaseType) => {
      updateInteractiveImportItem(id, { releaseType });
      onReprocessItems([id]);

      setSelectModalOpen(null);
      selectRowAfterChange();
    },
    [
      id,
      updateInteractiveImportItem,
      onReprocessItems,
      setSelectModalOpen,
      selectRowAfterChange,
    ]
  );

  const onSelectIndexerFlagsPress = useCallback(() => {
    setSelectModalOpen('indexerFlags');
  }, [setSelectModalOpen]);

  const onIndexerFlagsSelect = useCallback(
    (indexerFlags: number) => {
      updateInteractiveImportItem(id, { indexerFlags });
      onReprocessItems([id]);

      setSelectModalOpen(null);
      selectRowAfterChange();
    },
    [
      id,
      updateInteractiveImportItem,
      onReprocessItems,
      setSelectModalOpen,
      selectRowAfterChange,
    ]
  );

  const seriesTitle = series ? series.title : '';
  const isAnime = series?.seriesType === 'anime';

  const episodeInfo = episodes.map((episode) => {
    return (
      <div key={episode.id}>
        {episode.episodeNumber}

        {isAnime && episode.absoluteEpisodeNumber != null
          ? ` (${episode.absoluteEpisodeNumber})`
          : ''}

        {` - ${episode.title}`}
      </div>
    );
  });

  const requiresSeasonNumber = isNaN(Number(seasonNumber));
  const showSeriesPlaceholder = isSelected && !series;
  const showSeasonNumberPlaceholder =
    isSelected && !!series && requiresSeasonNumber && !isReprocessing;
  const showEpisodeNumbersPlaceholder =
    isSelected && Number.isInteger(seasonNumber) && !episodes.length;
  const showReleaseGroupPlaceholder = isSelected && !releaseGroup;
  const showQualityPlaceholder = isSelected && !quality;
  const showLanguagePlaceholder = isSelected && !languages;
  const showIndexerFlagsPlaceholder = isSelected && !indexerFlags;

  return (
    <TableRow>
      <TableSelectCell
        id={id}
        isSelected={isSelected}
        onSelectedChange={handleSelectedChange}
      />

      <TableRowCell className={styles.relativePath} title={relativePath}>
        {relativePath}
      </TableRowCell>

      {isSeriesColumnVisible ? (
        <TableRowCellButton
          isDisabled={!allowSeriesChange}
          title={
            allowSeriesChange ? translate('ClickToChangeSeries') : undefined
          }
          onPress={onSelectSeriesPress}
        >
          {showSeriesPlaceholder ? (
            <InteractiveImportRowCellPlaceholder />
          ) : (
            seriesTitle
          )}
        </TableRowCellButton>
      ) : null}

      <TableRowCellButton
        isDisabled={!series}
        title={series ? translate('ClickToChangeSeason') : undefined}
        onPress={onSelectSeasonPress}
      >
        {showSeasonNumberPlaceholder ? (
          <InteractiveImportRowCellPlaceholder />
        ) : (
          seasonNumber
        )}

        {isReprocessing && seasonNumber == null ? (
          <LoadingIndicator className={styles.reprocessing} size={20} />
        ) : null}
      </TableRowCellButton>

      <TableRowCellButton
        isDisabled={!series || requiresSeasonNumber}
        title={
          series && !requiresSeasonNumber
            ? translate('ClickToChangeEpisode')
            : undefined
        }
        onPress={onSelectEpisodePress}
      >
        {showEpisodeNumbersPlaceholder ? (
          <InteractiveImportRowCellPlaceholder />
        ) : (
          episodeInfo
        )}
      </TableRowCellButton>

      <TableRowCellButton
        title={translate('ClickToChangeReleaseGroup')}
        onPress={onSelectReleaseGroupPress}
      >
        {showReleaseGroupPlaceholder ? (
          <InteractiveImportRowCellPlaceholder isOptional={true} />
        ) : (
          releaseGroup
        )}
      </TableRowCellButton>

      <TableRowCellButton
        className={styles.quality}
        title={translate('ClickToChangeQuality')}
        onPress={onSelectQualityPress}
      >
        {showQualityPlaceholder && <InteractiveImportRowCellPlaceholder />}

        {!showQualityPlaceholder && !!quality && (
          <EpisodeQuality className={styles.label} quality={quality} />
        )}
      </TableRowCellButton>

      <TableRowCellButton
        className={styles.languages}
        title={translate('ClickToChangeLanguage')}
        onPress={onSelectLanguagePress}
      >
        {showLanguagePlaceholder && <InteractiveImportRowCellPlaceholder />}

        {!showLanguagePlaceholder && !!languages && (
          <EpisodeLanguages className={styles.label} languages={languages} />
        )}
      </TableRowCellButton>

      <TableRowCell>{formatBytes(size)}</TableRowCell>

      <TableRowCellButton
        title={translate('ClickToChangeReleaseType')}
        onPress={onSelectReleaseTypePress}
      >
        {getReleaseTypeName(releaseType)}
      </TableRowCellButton>

      <TableRowCell>
        {customFormats?.length ? (
          <Popover
            anchor={formatCustomFormatScore(
              customFormatScore,
              customFormats.length
            )}
            title={translate('CustomFormats')}
            body={
              <div className={styles.customFormatTooltip}>
                <EpisodeFormats formats={customFormats} />
              </div>
            }
            position={tooltipPositions.LEFT}
          />
        ) : null}
      </TableRowCell>

      {isIndexerFlagsColumnVisible ? (
        <TableRowCellButton
          title={translate('ClickToChangeIndexerFlags')}
          onPress={onSelectIndexerFlagsPress}
        >
          {showIndexerFlagsPlaceholder ? (
            <InteractiveImportRowCellPlaceholder isOptional={true} />
          ) : (
            <>
              {indexerFlags ? (
                <Popover
                  anchor={<Icon name={icons.FLAG} />}
                  title={translate('IndexerFlags')}
                  body={<IndexerFlags indexerFlags={indexerFlags} />}
                  position={tooltipPositions.LEFT}
                />
              ) : null}
            </>
          )}
        </TableRowCellButton>
      ) : null}

      <TableRowCell>
        {rejections.length ? (
          <Popover
            anchor={<Icon name={icons.DANGER} kind={kinds.DANGER} />}
            title={translate('ReleaseRejected')}
            body={
              <ul>
                {rejections.map((rejection, index) => {
                  return <li key={index}>{rejection.message}</li>;
                })}
              </ul>
            }
            position={tooltipPositions.LEFT}
            canFlip={false}
          />
        ) : null}
      </TableRowCell>

      <SelectSeriesModal
        isOpen={selectModalOpen === 'series'}
        modalTitle={modalTitle}
        onSeriesSelect={onSeriesSelect}
        onModalClose={onSelectModalClose}
      />

      <SelectSeasonModal
        isOpen={selectModalOpen === 'season'}
        seriesId={series?.id}
        modalTitle={modalTitle}
        onSeasonSelect={onSeasonSelect}
        onModalClose={onSelectModalClose}
      />

      <SelectEpisodeModal
        isOpen={selectModalOpen === 'episode'}
        selectedIds={[id]}
        seriesId={series?.id}
        isAnime={isAnime}
        seasonNumber={seasonNumber}
        selectedDetails={relativePath}
        modalTitle={modalTitle}
        onEpisodesSelect={onEpisodesSelect}
        onModalClose={onSelectModalClose}
      />

      <SelectReleaseGroupModal
        isOpen={selectModalOpen === 'releaseGroup'}
        releaseGroup={releaseGroup ?? ''}
        modalTitle={modalTitle}
        onReleaseGroupSelect={onReleaseGroupSelect}
        onModalClose={onSelectModalClose}
      />

      <SelectQualityModal
        isOpen={selectModalOpen === 'quality'}
        qualityId={quality ? quality.quality.id : 0}
        proper={quality ? quality.revision.version > 1 : false}
        real={quality ? quality.revision.real > 0 : false}
        modalTitle={modalTitle}
        onQualitySelect={onQualitySelect}
        onModalClose={onSelectModalClose}
      />

      <SelectLanguageModal
        isOpen={selectModalOpen === 'language'}
        languageIds={languages ? languages.map((l) => l.id) : []}
        modalTitle={modalTitle}
        onLanguagesSelect={onLanguagesSelect}
        onModalClose={onSelectModalClose}
      />

      <SelectReleaseTypeModal
        isOpen={selectModalOpen === 'releaseType'}
        releaseType={releaseType ?? 'unknown'}
        modalTitle={modalTitle}
        onReleaseTypeSelect={onReleaseTypeSelect}
        onModalClose={onSelectModalClose}
      />

      <SelectIndexerFlagsModal
        isOpen={selectModalOpen === 'indexerFlags'}
        indexerFlags={indexerFlags ?? 0}
        modalTitle={modalTitle}
        onIndexerFlagsSelect={onIndexerFlagsSelect}
        onModalClose={onSelectModalClose}
      />
    </TableRow>
  );
}

export default InteractiveImportRow;
