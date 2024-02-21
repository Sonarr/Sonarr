import { cloneDeep, without } from 'lodash';
import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import InteractiveImportAppState from 'App/State/InteractiveImportAppState';
import * as commandNames from 'Commands/commandNames';
import SelectInput from 'Components/Form/SelectInput';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import SpinnerButton from 'Components/Link/SpinnerButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Menu from 'Components/Menu/Menu';
import MenuButton from 'Components/Menu/MenuButton';
import MenuContent from 'Components/Menu/MenuContent';
import SelectedMenuItem from 'Components/Menu/SelectedMenuItem';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import Column from 'Components/Table/Column';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { EpisodeFile } from 'EpisodeFile/EpisodeFile';
import usePrevious from 'Helpers/Hooks/usePrevious';
import useSelectState from 'Helpers/Hooks/useSelectState';
import { align, icons, kinds, scrollDirections } from 'Helpers/Props';
import SelectEpisodeModal from 'InteractiveImport/Episode/SelectEpisodeModal';
import { SelectedEpisode } from 'InteractiveImport/Episode/SelectEpisodeModalContent';
import ImportMode from 'InteractiveImport/ImportMode';
import SelectIndexerFlagsModal from 'InteractiveImport/IndexerFlags/SelectIndexerFlagsModal';
import InteractiveImport, {
  InteractiveImportCommandOptions,
} from 'InteractiveImport/InteractiveImport';
import SelectLanguageModal from 'InteractiveImport/Language/SelectLanguageModal';
import SelectQualityModal from 'InteractiveImport/Quality/SelectQualityModal';
import SelectReleaseGroupModal from 'InteractiveImport/ReleaseGroup/SelectReleaseGroupModal';
import SelectSeasonModal from 'InteractiveImport/Season/SelectSeasonModal';
import SelectSeriesModal from 'InteractiveImport/Series/SelectSeriesModal';
import Language from 'Language/Language';
import { QualityModel } from 'Quality/Quality';
import Series from 'Series/Series';
import { executeCommand } from 'Store/Actions/commandActions';
import {
  deleteEpisodeFiles,
  updateEpisodeFiles,
} from 'Store/Actions/episodeFileActions';
import {
  clearInteractiveImport,
  fetchInteractiveImportItems,
  reprocessInteractiveImportItems,
  setInteractiveImportMode,
  setInteractiveImportSort,
  updateInteractiveImportItem,
  updateInteractiveImportItems,
} from 'Store/Actions/interactiveImportActions';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import { SortCallback } from 'typings/callbacks';
import { SelectStateInputProps } from 'typings/props';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import hasDifferentItems from 'Utilities/Object/hasDifferentItems';
import translate from 'Utilities/String/translate';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
import InteractiveImportRow from './InteractiveImportRow';
import styles from './InteractiveImportModalContent.css';

type SelectType =
  | 'select'
  | 'series'
  | 'season'
  | 'episode'
  | 'releaseGroup'
  | 'quality'
  | 'language'
  | 'indexerFlags';

type FilterExistingFiles = 'all' | 'new';

// TODO: This feels janky to do, but not sure of a better way currently
type OnSelectedChangeCallback = React.ComponentProps<
  typeof InteractiveImportRow
>['onSelectedChange'];

const COLUMNS = [
  {
    name: 'relativePath',
    label: () => translate('RelativePath'),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'series',
    label: () => translate('Series'),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'season',
    label: () => translate('Season'),
    isVisible: true,
  },
  {
    name: 'episodes',
    label: () => translate('Episodes'),
    isVisible: true,
  },
  {
    name: 'releaseGroup',
    label: () => translate('ReleaseGroup'),
    isVisible: true,
  },
  {
    name: 'quality',
    label: () => translate('Quality'),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'languages',
    label: () => translate('Languages'),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'size',
    label: () => translate('Size'),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'customFormats',
    label: React.createElement(Icon, {
      name: icons.INTERACTIVE,
      title: () => translate('CustomFormatScore'),
    }),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'indexerFlags',
    label: React.createElement(Icon, {
      name: icons.FLAG,
      title: () => translate('IndexerFlags'),
    }),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'rejections',
    label: React.createElement(Icon, {
      name: icons.DANGER,
      kind: kinds.DANGER,
      title: () => translate('Rejections'),
    }),
    isSortable: true,
    isVisible: true,
  },
];

const importModeOptions = [
  {
    key: 'chooseImportMode',
    value: () => translate('ChooseImportMode'),
    disabled: true,
  },
  {
    key: 'move',
    value: () => translate('MoveFiles'),
  },
  {
    key: 'copy',
    value: () => translate('HardlinkCopyFiles'),
  },
];

function isSameEpisodeFile(
  file: InteractiveImport,
  originalFile?: InteractiveImport
) {
  const { series, seasonNumber, episodes } = file;

  if (!originalFile) {
    return false;
  }

  if (!originalFile.series || series?.id !== originalFile.series.id) {
    return false;
  }

  if (seasonNumber !== originalFile.seasonNumber) {
    return false;
  }

  return !hasDifferentItems(originalFile.episodes, episodes);
}

const episodeFilesInfoSelector = createSelector(
  (state: AppState) => state.episodeFiles.isDeleting,
  (state: AppState) => state.episodeFiles.deleteError,
  (isDeleting, deleteError) => {
    return {
      isDeleting,
      deleteError,
    };
  }
);

const importModeSelector = createSelector(
  (state: AppState) => state.interactiveImport.importMode,
  (importMode) => {
    return importMode;
  }
);

interface InteractiveImportModalContentProps {
  downloadId?: string;
  seriesId?: number;
  seasonNumber?: number;
  showSeries?: boolean;
  allowSeriesChange?: boolean;
  showDelete?: boolean;
  showImportMode?: boolean;
  showFilterExistingFiles?: boolean;
  title?: string;
  folder?: string;
  sortKey?: string;
  sortDirection?: string;
  initialSortKey?: string;
  initialSortDirection?: string;
  modalTitle: string;
  onModalClose(): void;
}

function InteractiveImportModalContent(
  props: InteractiveImportModalContentProps
) {
  const {
    downloadId,
    seriesId,
    seasonNumber,
    allowSeriesChange = true,
    showSeries = true,
    showFilterExistingFiles = false,
    showDelete = false,
    showImportMode = true,
    title,
    folder,
    initialSortKey,
    initialSortDirection,
    modalTitle,
    onModalClose,
  } = props;

  const {
    isFetching,
    isPopulated,
    error,
    items,
    originalItems,
    sortKey,
    sortDirection,
  }: InteractiveImportAppState = useSelector(
    createClientSideCollectionSelector('interactiveImport')
  );

  const { isDeleting, deleteError } = useSelector(episodeFilesInfoSelector);
  const importMode = useSelector(importModeSelector);

  const [invalidRowsSelected, setInvalidRowsSelected] = useState<number[]>([]);
  const [
    withoutEpisodeFileIdRowsSelected,
    setWithoutEpisodeFileIdRowsSelected,
  ] = useState<number[]>([]);
  const [selectModalOpen, setSelectModalOpen] = useState<SelectType | null>(
    null
  );
  const [isConfirmDeleteModalOpen, setIsConfirmDeleteModalOpen] =
    useState(false);
  const [filterExistingFiles, setFilterExistingFiles] = useState(false);
  const [interactiveImportErrorMessage, setInteractiveImportErrorMessage] =
    useState<string | null>(null);
  const [selectState, setSelectState] = useSelectState();
  const { allSelected, allUnselected, selectedState } = selectState;
  const previousIsDeleting = usePrevious(isDeleting);
  const dispatch = useDispatch();

  const columns: Column[] = useMemo(() => {
    const result: Column[] = cloneDeep(COLUMNS);

    if (!showSeries) {
      const seriesColumn = result.find((c) => c.name === 'series');

      if (seriesColumn) {
        seriesColumn.isVisible = false;
      }
    }

    const showIndexerFlags = items.some((item) => item.indexerFlags);

    if (!showIndexerFlags) {
      const indexerFlagsColumn = result.find((c) => c.name === 'indexerFlags');

      if (indexerFlagsColumn) {
        indexerFlagsColumn.isVisible = false;
      }
    }

    return result;
  }, [showSeries, items]);

  const selectedIds: number[] = useMemo(() => {
    return getSelectedIds(selectedState);
  }, [selectedState]);

  const bulkSelectOptions = useMemo(() => {
    const { seasonSelectDisabled, episodeSelectDisabled } = items.reduce(
      (acc, item) => {
        if (!selectedIds.includes(item.id)) {
          return acc;
        }

        const lastSelectedSeason = acc.lastSelectedSeason;

        acc.seasonSelectDisabled ||= !item.series;
        acc.episodeSelectDisabled ||=
          item.seasonNumber === undefined ||
          (lastSelectedSeason >= 0 && item.seasonNumber !== lastSelectedSeason);
        acc.lastSelectedSeason = item.seasonNumber ?? -1;

        return acc;
      },
      {
        seasonSelectDisabled: false,
        episodeSelectDisabled: false,
        lastSelectedSeason: -1,
      }
    );

    const options = [
      {
        key: 'select',
        value: translate('SelectDropdown'),
        disabled: true,
      },
      {
        key: 'season',
        value: translate('SelectSeason'),
        disabled: seasonSelectDisabled,
      },
      {
        key: 'episode',
        value: translate('SelectEpisodes'),
        disabled: episodeSelectDisabled,
      },
      {
        key: 'quality',
        value: translate('SelectQuality'),
      },
      {
        key: 'releaseGroup',
        value: translate('SelectReleaseGroup'),
      },
      {
        key: 'language',
        value: translate('SelectLanguage'),
      },
      {
        key: 'indexerFlags',
        value: translate('SelectIndexerFlags'),
      },
    ];

    if (allowSeriesChange) {
      options.splice(1, 0, {
        key: 'series',
        value: translate('SelectSeries'),
      });
    }

    return options;
  }, [allowSeriesChange, items, selectedIds]);

  useEffect(
    () => {
      if (initialSortKey) {
        const sortProps: { sortKey: string; sortDirection?: string } = {
          sortKey: initialSortKey,
        };

        if (initialSortDirection) {
          sortProps.sortDirection = initialSortDirection;
        }

        dispatch(setInteractiveImportSort(sortProps));
      }

      dispatch(
        fetchInteractiveImportItems({
          downloadId,
          seriesId,
          seasonNumber,
          folder,
          filterExistingFiles,
        })
      );

      // returned function will be called on component unmount
      return () => {
        dispatch(clearInteractiveImport());
      };
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    []
  );

  useEffect(() => {
    if (!isDeleting && previousIsDeleting && !deleteError) {
      onModalClose();
    }
  }, [previousIsDeleting, isDeleting, deleteError, onModalClose]);

  const onSelectAllChange = useCallback(
    ({ value }: SelectStateInputProps) => {
      setSelectState({ type: value ? 'selectAll' : 'unselectAll', items });
    },
    [items, setSelectState]
  );

  const onSelectedChange = useCallback<OnSelectedChangeCallback>(
    ({ id, value, hasEpisodeFileId, shiftKey = false }) => {
      setSelectState({
        type: 'toggleSelected',
        items,
        id,
        isSelected: value,
        shiftKey,
      });

      setWithoutEpisodeFileIdRowsSelected(
        hasEpisodeFileId || !value
          ? without(withoutEpisodeFileIdRowsSelected, id)
          : [...withoutEpisodeFileIdRowsSelected, id]
      );
    },
    [
      items,
      withoutEpisodeFileIdRowsSelected,
      setSelectState,
      setWithoutEpisodeFileIdRowsSelected,
    ]
  );

  const onValidRowChange = useCallback(
    (id: number, isValid: boolean) => {
      if (isValid && invalidRowsSelected.includes(id)) {
        setInvalidRowsSelected(without(invalidRowsSelected, id));
      } else if (!isValid && !invalidRowsSelected.includes(id)) {
        setInvalidRowsSelected([...invalidRowsSelected, id]);
      }
    },
    [invalidRowsSelected, setInvalidRowsSelected]
  );

  const onDeleteSelectedPress = useCallback(() => {
    setIsConfirmDeleteModalOpen(true);
  }, [setIsConfirmDeleteModalOpen]);

  const onConfirmDelete = useCallback(() => {
    setIsConfirmDeleteModalOpen(false);

    const episodeFileIds = items.reduce((acc: number[], item) => {
      if (selectedIds.indexOf(item.id) > -1 && item.episodeFileId) {
        acc.push(item.episodeFileId);
      }

      return acc;
    }, []);

    dispatch(deleteEpisodeFiles({ episodeFileIds }));
  }, [items, selectedIds, setIsConfirmDeleteModalOpen, dispatch]);

  const onConfirmDeleteModalClose = useCallback(() => {
    setIsConfirmDeleteModalOpen(false);
  }, [setIsConfirmDeleteModalOpen]);

  const onImportSelectedPress = useCallback(() => {
    const finalImportMode = downloadId || !showImportMode ? 'auto' : importMode;

    const existingFiles: Partial<EpisodeFile>[] = [];
    const files: InteractiveImportCommandOptions[] = [];

    if (finalImportMode === 'chooseImportMode') {
      setInteractiveImportErrorMessage(
        translate('InteractiveImportNoImportMode')
      );

      return;
    }

    items.forEach((item) => {
      const isSelected = selectedIds.indexOf(item.id) > -1;

      if (isSelected) {
        const {
          series,
          seasonNumber,
          episodes,
          releaseGroup,
          quality,
          languages,
          indexerFlags,
          episodeFileId,
        } = item;

        if (!series) {
          setInteractiveImportErrorMessage(
            translate('InteractiveImportNoSeries')
          );
          return;
        }

        if (isNaN(seasonNumber)) {
          setInteractiveImportErrorMessage(
            translate('InteractiveImportNoSeason')
          );
          return;
        }

        if (!episodes || !episodes.length) {
          setInteractiveImportErrorMessage(
            translate('InteractiveImportNoEpisode')
          );
          return;
        }

        if (!quality) {
          setInteractiveImportErrorMessage(
            translate('InteractiveImportNoQuality')
          );
          return;
        }

        if (!languages) {
          setInteractiveImportErrorMessage(
            translate('InteractiveImportNoLanguage')
          );
          return;
        }

        setInteractiveImportErrorMessage(null);

        if (episodeFileId) {
          const originalItem = originalItems.find((i) => i.id === item.id);

          if (isSameEpisodeFile(item, originalItem)) {
            existingFiles.push({
              id: episodeFileId,
              releaseGroup,
              quality,
              languages,
              indexerFlags,
            });

            return;
          }
        }

        files.push({
          path: item.path,
          folderName: item.folderName,
          seriesId: series.id,
          episodeIds: episodes.map((e) => e.id),
          releaseGroup,
          quality,
          languages,
          indexerFlags,
          downloadId,
          episodeFileId,
        });
      }
    });

    let shouldClose = false;

    if (existingFiles.length) {
      dispatch(
        updateEpisodeFiles({
          files: existingFiles,
        })
      );

      shouldClose = true;
    }

    if (files.length) {
      dispatch(
        executeCommand({
          name: commandNames.INTERACTIVE_IMPORT,
          files,
          importMode: finalImportMode,
        })
      );

      shouldClose = true;
    }

    if (shouldClose) {
      onModalClose();
    }
  }, [
    downloadId,
    showImportMode,
    importMode,
    items,
    originalItems,
    selectedIds,
    onModalClose,
    dispatch,
  ]);

  const onSortPress = useCallback<SortCallback>(
    (sortKey, sortDirection) => {
      dispatch(setInteractiveImportSort({ sortKey, sortDirection }));
    },
    [dispatch]
  );

  const onFilterExistingFilesChange = useCallback<
    (value: FilterExistingFiles) => void
  >(
    (value) => {
      const filter = value !== 'all';

      setFilterExistingFiles(filter);

      dispatch(
        fetchInteractiveImportItems({
          downloadId,
          seriesId,
          folder,
          filterExistingFiles: filter,
        })
      );
    },
    [downloadId, seriesId, folder, setFilterExistingFiles, dispatch]
  );

  const onImportModeChange = useCallback<
    ({ value }: { value: ImportMode }) => void
  >(
    ({ value }) => {
      dispatch(setInteractiveImportMode({ importMode: value }));
    },
    [dispatch]
  );

  const onSelectModalSelect = useCallback<
    ({ value }: { value: SelectType }) => void
  >(
    ({ value }) => {
      setSelectModalOpen(value);
    },
    [setSelectModalOpen]
  );

  const onSelectModalClose = useCallback(() => {
    setSelectModalOpen(null);
  }, [setSelectModalOpen]);

  const onSeriesSelect = useCallback(
    (series: Series) => {
      dispatch(
        updateInteractiveImportItems({
          ids: selectedIds,
          series,
          seasonNumber: undefined,
          episodes: [],
        })
      );

      dispatch(reprocessInteractiveImportItems({ ids: selectedIds }));

      setSelectModalOpen(null);
    },
    [selectedIds, setSelectModalOpen, dispatch]
  );

  const onSeasonSelect = useCallback(
    (seasonNumber: number) => {
      dispatch(
        updateInteractiveImportItems({
          ids: selectedIds,
          seasonNumber,
          episodes: [],
        })
      );

      dispatch(reprocessInteractiveImportItems({ ids: selectedIds }));

      setSelectModalOpen(null);
    },
    [selectedIds, setSelectModalOpen, dispatch]
  );

  const onEpisodesSelect = useCallback(
    (selectedEpisodes: SelectedEpisode[]) => {
      selectedEpisodes.forEach((selectedEpisode) => {
        const { id, episodes } = selectedEpisode;

        dispatch(
          updateInteractiveImportItem({
            id,
            episodes,
          })
        );
      });

      dispatch(reprocessInteractiveImportItems({ ids: selectedIds }));

      setSelectModalOpen(null);
    },
    [selectedIds, setSelectModalOpen, dispatch]
  );

  const onReleaseGroupSelect = useCallback(
    (releaseGroup: string) => {
      dispatch(
        updateInteractiveImportItems({
          ids: selectedIds,
          releaseGroup,
        })
      );

      dispatch(reprocessInteractiveImportItems({ ids: selectedIds }));

      setSelectModalOpen(null);
    },
    [selectedIds, dispatch]
  );

  const onLanguagesSelect = useCallback(
    (newLanguages: Language[]) => {
      dispatch(
        updateInteractiveImportItems({
          ids: selectedIds,
          languages: newLanguages,
        })
      );

      dispatch(reprocessInteractiveImportItems({ ids: selectedIds }));

      setSelectModalOpen(null);
    },
    [selectedIds, dispatch]
  );

  const onQualitySelect = useCallback(
    (quality: QualityModel) => {
      dispatch(
        updateInteractiveImportItems({
          ids: selectedIds,
          quality,
        })
      );

      dispatch(reprocessInteractiveImportItems({ ids: selectedIds }));

      setSelectModalOpen(null);
    },
    [selectedIds, dispatch]
  );

  const onIndexerFlagsSelect = useCallback(
    (indexerFlags: number) => {
      dispatch(
        updateInteractiveImportItems({
          ids: selectedIds,
          indexerFlags,
        })
      );

      dispatch(reprocessInteractiveImportItems({ ids: selectedIds }));

      setSelectModalOpen(null);
    },
    [selectedIds, dispatch]
  );

  const orderedSelectedIds = items.reduce((acc: number[], file) => {
    if (selectedIds.includes(file.id)) {
      acc.push(file.id);
    }

    return acc;
  }, []);

  const selectedItem = selectedIds.length
    ? items.find((file) => file.id === selectedIds[0])
    : null;

  const errorMessage = getErrorMessage(
    error,
    translate('InteractiveImportLoadError')
  );

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {modalTitle} - {title || folder}
      </ModalHeader>

      <ModalBody scrollDirection={scrollDirections.BOTH}>
        {showFilterExistingFiles && (
          <div className={styles.filterContainer}>
            <Menu alignMenu={align.RIGHT}>
              <MenuButton>
                <Icon name={icons.FILTER} size={22} />

                <div className={styles.filterText}>
                  {filterExistingFiles
                    ? translate('UnmappedFilesOnly')
                    : translate('AllFiles')}
                </div>
              </MenuButton>

              <MenuContent>
                <SelectedMenuItem
                  name={'all'}
                  isSelected={!filterExistingFiles}
                  onPress={onFilterExistingFilesChange}
                >
                  {translate('AllFiles')}
                </SelectedMenuItem>

                <SelectedMenuItem
                  name={'new'}
                  isSelected={filterExistingFiles}
                  onPress={onFilterExistingFilesChange}
                >
                  {translate('UnmappedFilesOnly')}
                </SelectedMenuItem>
              </MenuContent>
            </Menu>
          </div>
        )}

        {isFetching ? <LoadingIndicator /> : null}

        {error ? <div>{errorMessage}</div> : null}

        {isPopulated && !!items.length && !isFetching && !isFetching ? (
          <Table
            columns={columns}
            horizontalScroll={true}
            selectAll={true}
            allSelected={allSelected}
            allUnselected={allUnselected}
            sortKey={sortKey}
            sortDirection={sortDirection}
            onSortPress={onSortPress}
            onSelectAllChange={onSelectAllChange}
          >
            <TableBody>
              {items.map((item) => {
                return (
                  <InteractiveImportRow
                    key={item.id}
                    isSelected={selectedState[item.id]}
                    {...item}
                    allowSeriesChange={allowSeriesChange}
                    columns={columns}
                    modalTitle={modalTitle}
                    onSelectedChange={onSelectedChange}
                    onValidRowChange={onValidRowChange}
                  />
                );
              })}
            </TableBody>
          </Table>
        ) : null}

        {isPopulated && !items.length && !isFetching
          ? translate('InteractiveImportNoFilesFound')
          : null}
      </ModalBody>

      <ModalFooter className={styles.footer}>
        <div className={styles.leftButtons}>
          {showDelete ? (
            <SpinnerButton
              className={styles.deleteButton}
              kind={kinds.DANGER}
              isSpinning={isDeleting}
              isDisabled={
                !selectedIds.length || !!withoutEpisodeFileIdRowsSelected.length
              }
              onPress={onDeleteSelectedPress}
            >
              {translate('Delete')}
            </SpinnerButton>
          ) : null}

          {!downloadId && showImportMode ? (
            <SelectInput
              className={styles.importMode}
              name="importMode"
              value={importMode}
              values={importModeOptions}
              onChange={onImportModeChange}
            />
          ) : null}

          <SelectInput
            className={styles.bulkSelect}
            name="select"
            value={'select'}
            values={bulkSelectOptions}
            isDisabled={!selectedIds.length}
            onChange={onSelectModalSelect}
          />
        </div>

        <div className={styles.rightButtons}>
          <Button onPress={onModalClose}>Cancel</Button>

          {interactiveImportErrorMessage && (
            <span className={styles.errorMessage}>
              {interactiveImportErrorMessage}
            </span>
          )}

          <Button
            kind={kinds.SUCCESS}
            isDisabled={!selectedIds.length || !!invalidRowsSelected.length}
            onPress={onImportSelectedPress}
          >
            {translate('Import')}
          </Button>
        </div>
      </ModalFooter>

      <SelectSeriesModal
        isOpen={selectModalOpen === 'series'}
        modalTitle={modalTitle}
        onSeriesSelect={onSeriesSelect}
        onModalClose={onSelectModalClose}
      />

      <SelectSeasonModal
        isOpen={selectModalOpen === 'season'}
        seriesId={selectedItem?.series?.id}
        modalTitle={modalTitle}
        onSeasonSelect={onSeasonSelect}
        onModalClose={onSelectModalClose}
      />

      <SelectEpisodeModal
        isOpen={selectModalOpen === 'episode'}
        selectedIds={orderedSelectedIds}
        seriesId={selectedItem?.series?.id}
        seasonNumber={selectedItem?.seasonNumber}
        isAnime={selectedItem?.series?.seriesType === 'anime'}
        modalTitle={modalTitle}
        onEpisodesSelect={onEpisodesSelect}
        onModalClose={onSelectModalClose}
      />

      <SelectReleaseGroupModal
        isOpen={selectModalOpen === 'releaseGroup'}
        releaseGroup=""
        modalTitle={modalTitle}
        onReleaseGroupSelect={onReleaseGroupSelect}
        onModalClose={onSelectModalClose}
      />

      <SelectLanguageModal
        isOpen={selectModalOpen === 'language'}
        languageIds={[0]}
        modalTitle={modalTitle}
        onLanguagesSelect={onLanguagesSelect}
        onModalClose={onSelectModalClose}
      />

      <SelectQualityModal
        isOpen={selectModalOpen === 'quality'}
        qualityId={0}
        proper={false}
        real={false}
        modalTitle={modalTitle}
        onQualitySelect={onQualitySelect}
        onModalClose={onSelectModalClose}
      />

      <SelectIndexerFlagsModal
        isOpen={selectModalOpen === 'indexerFlags'}
        indexerFlags={0}
        modalTitle={modalTitle}
        onIndexerFlagsSelect={onIndexerFlagsSelect}
        onModalClose={onSelectModalClose}
      />

      <ConfirmModal
        isOpen={isConfirmDeleteModalOpen}
        kind={kinds.DANGER}
        title={translate('DeleteSelectedEpisodeFiles')}
        message={translate('DeleteSelectedEpisodeFilesHelpText')}
        confirmLabel={translate('Delete')}
        onConfirm={onConfirmDelete}
        onCancel={onConfirmDeleteModalClose}
      />
    </ModalContent>
  );
}

export default InteractiveImportModalContent;
