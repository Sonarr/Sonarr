import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { useDispatch } from 'react-redux';
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
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import SelectEpisodeModal from 'InteractiveImport/Episode/SelectEpisodeModal';
import { SelectedEpisode } from 'InteractiveImport/Episode/SelectEpisodeModalContent';
import SelectLanguageModal from 'InteractiveImport/Language/SelectLanguageModal';
import SelectQualityModal from 'InteractiveImport/Quality/SelectQualityModal';
import SelectReleaseGroupModal from 'InteractiveImport/ReleaseGroup/SelectReleaseGroupModal';
import SelectSeasonModal from 'InteractiveImport/Season/SelectSeasonModal';
import SelectSeriesModal from 'InteractiveImport/Series/SelectSeriesModal';
import Language from 'Language/Language';
import { QualityModel } from 'Quality/Quality';
import Series from 'Series/Series';
import {
  reprocessInteractiveImportItems,
  updateInteractiveImportItem,
} from 'Store/Actions/interactiveImportActions';
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
  | 'language';

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
  customFormats?: object[];
  customFormatScore?: number;
  rejections: Rejection[];
  columns: Column[];
  episodeFileId?: number;
  isReprocessing?: boolean;
  isSelected?: boolean;
  modalTitle: string;
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
    customFormats,
    customFormatScore,
    rejections,
    isReprocessing,
    isSelected,
    modalTitle,
    episodeFileId,
    columns,
    onSelectedChange,
    onValidRowChange,
  } = props;

  const dispatch = useDispatch();

  const isSeriesColumnVisible = useMemo(
    () => columns.find((c) => c.name === 'series')?.isVisible ?? false,
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
        languages
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

  const onSelectedChangeWrapper = useCallback(
    (result: SelectedChangeProps) => {
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
      dispatch(
        updateInteractiveImportItem({
          id,
          series,
          seasonNumber: undefined,
          episodes: [],
        })
      );

      dispatch(reprocessInteractiveImportItems({ ids: [id] }));

      setSelectModalOpen(null);
      selectRowAfterChange();
    },
    [id, dispatch, setSelectModalOpen, selectRowAfterChange]
  );

  const onSelectSeasonPress = useCallback(() => {
    setSelectModalOpen('season');
  }, [setSelectModalOpen]);

  const onSeasonSelect = useCallback(
    (seasonNumber: number) => {
      dispatch(
        updateInteractiveImportItem({
          id,
          seasonNumber,
          episodes: [],
        })
      );

      dispatch(reprocessInteractiveImportItems({ ids: [id] }));

      setSelectModalOpen(null);
      selectRowAfterChange();
    },
    [id, dispatch, setSelectModalOpen, selectRowAfterChange]
  );

  const onSelectEpisodePress = useCallback(() => {
    setSelectModalOpen('episode');
  }, [setSelectModalOpen]);

  const onEpisodesSelect = useCallback(
    (selectedEpisodes: SelectedEpisode[]) => {
      dispatch(
        updateInteractiveImportItem({
          id,
          episodes: selectedEpisodes[0].episodes,
        })
      );

      dispatch(reprocessInteractiveImportItems({ ids: [id] }));

      setSelectModalOpen(null);
      selectRowAfterChange();
    },
    [id, dispatch, setSelectModalOpen, selectRowAfterChange]
  );

  const onSelectReleaseGroupPress = useCallback(() => {
    setSelectModalOpen('releaseGroup');
  }, [setSelectModalOpen]);

  const onReleaseGroupSelect = useCallback(
    (releaseGroup: string) => {
      dispatch(
        updateInteractiveImportItem({
          id,
          releaseGroup,
        })
      );

      dispatch(reprocessInteractiveImportItems({ ids: [id] }));

      setSelectModalOpen(null);
      selectRowAfterChange();
    },
    [id, dispatch, setSelectModalOpen, selectRowAfterChange]
  );

  const onSelectQualityPress = useCallback(() => {
    setSelectModalOpen('quality');
  }, [setSelectModalOpen]);

  const onQualitySelect = useCallback(
    (quality: QualityModel) => {
      dispatch(
        updateInteractiveImportItem({
          id,
          quality,
        })
      );

      dispatch(reprocessInteractiveImportItems({ ids: [id] }));

      setSelectModalOpen(null);
      selectRowAfterChange();
    },
    [id, dispatch, setSelectModalOpen, selectRowAfterChange]
  );

  const onSelectLanguagePress = useCallback(() => {
    setSelectModalOpen('language');
  }, [setSelectModalOpen]);

  const onLanguagesSelect = useCallback(
    (languages: Language[]) => {
      dispatch(
        updateInteractiveImportItem({
          id,
          languages,
        })
      );

      dispatch(reprocessInteractiveImportItems({ ids: [id] }));

      setSelectModalOpen(null);
      selectRowAfterChange();
    },
    [id, dispatch, setSelectModalOpen, selectRowAfterChange]
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

  return (
    <TableRow>
      <TableSelectCell
        id={id}
        isSelected={isSelected}
        onSelectedChange={onSelectedChangeWrapper}
      />

      <TableRowCell className={styles.relativePath} title={relativePath}>
        {relativePath}
      </TableRowCell>

      {isSeriesColumnVisible ? (
        <TableRowCellButton
          isDisabled={!allowSeriesChange}
          title={allowSeriesChange ? 'Click to change series' : undefined}
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
        title={series ? 'Click to change season' : undefined}
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
            ? 'Click to change episode'
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
        title="Click to change release group"
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
        title="Click to change quality"
        onPress={onSelectQualityPress}
      >
        {showQualityPlaceholder && <InteractiveImportRowCellPlaceholder />}

        {!showQualityPlaceholder && !!quality && (
          <EpisodeQuality className={styles.label} quality={quality} />
        )}
      </TableRowCellButton>

      <TableRowCellButton
        className={styles.languages}
        title="Click to change language"
        onPress={onSelectLanguagePress}
      >
        {showLanguagePlaceholder && <InteractiveImportRowCellPlaceholder />}

        {!showLanguagePlaceholder && !!languages && (
          <EpisodeLanguages className={styles.label} languages={languages} />
        )}
      </TableRowCellButton>

      <TableRowCell>{formatBytes(size)}</TableRowCell>

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

      <TableRowCell>
        {rejections.length ? (
          <Popover
            anchor={<Icon name={icons.DANGER} kind={kinds.DANGER} />}
            title="Release Rejected"
            body={
              <ul>
                {rejections.map((rejection, index) => {
                  return <li key={index}>{rejection.reason}</li>;
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
    </TableRow>
  );
}

export default InteractiveImportRow;
