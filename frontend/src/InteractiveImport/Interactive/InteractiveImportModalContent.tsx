import { cloneDeep, without } from 'lodash';
import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { create } from 'zustand';
import { SelectProvider, useSelect } from 'App/Select/SelectContext';
import CommandNames from 'Commands/CommandNames';
import { useExecuteCommand } from 'Commands/useCommands';
import SelectInput, { SelectInputOption } from 'Components/Form/SelectInput';
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
import {
  useDeleteEpisodeFiles,
  useUpdateEpisodeFiles,
} from 'EpisodeFile/useEpisodeFiles';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { align, icons, kinds, scrollDirections } from 'Helpers/Props';
import { SortDirection } from 'Helpers/Props/sortDirections';
import SelectEpisodeModal from 'InteractiveImport/Episode/SelectEpisodeModal';
import { SelectedEpisode } from 'InteractiveImport/Episode/SelectEpisodeModalContent';
import ImportMode from 'InteractiveImport/ImportMode';
import SelectIndexerFlagsModal from 'InteractiveImport/IndexerFlags/SelectIndexerFlagsModal';
import InteractiveImport, {
  InteractiveImportCommandOptions,
} from 'InteractiveImport/InteractiveImport';
import {
  setInteractiveImportOption,
  setInteractiveImportSort,
  useInteractiveImportOptions,
} from 'InteractiveImport/interactiveImportOptionsStore';
import SelectLanguageModal from 'InteractiveImport/Language/SelectLanguageModal';
import SelectQualityModal from 'InteractiveImport/Quality/SelectQualityModal';
import SelectReleaseGroupModal from 'InteractiveImport/ReleaseGroup/SelectReleaseGroupModal';
import ReleaseType from 'InteractiveImport/ReleaseType';
import SelectReleaseTypeModal from 'InteractiveImport/ReleaseType/SelectReleaseTypeModal';
import SelectSeasonModal from 'InteractiveImport/Season/SelectSeasonModal';
import SelectSeriesModal from 'InteractiveImport/Series/SelectSeriesModal';
import useInteractiveImport, {
  useReprocessInteractiveImportItems,
  useUpdateInteractiveImportItem,
  useUpdateInteractiveImportItems,
} from 'InteractiveImport/useInteractiveImport';
import Language from 'Language/Language';
import { QualityModel } from 'Quality/Quality';
import Series from 'Series/Series';
import { SortCallback } from 'typings/callbacks';
import { CheckInputChanged } from 'typings/inputs';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import hasDifferentItems from 'Utilities/Object/hasDifferentItems';
import translate from 'Utilities/String/translate';
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
  | 'indexerFlags'
  | 'releaseType';

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
    name: 'releaseType',
    label: () => translate('ReleaseType'),
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

const importModeOptions: SelectInputOption[] = [
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

const filterExistingFilesStore = create<boolean>(() => false);

export interface InteractiveImportModalContentProps {
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

function InteractiveImportModalContentInner(
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

  const filterExistingFiles = filterExistingFilesStore((state) => state);
  const [reprocessingItems, setReprocessingItems] = useState<Set<number>>(
    new Set()
  );

  const {
    isFetching,
    isFetched: isPopulated,
    error,
    data,
    originalItems,
  } = useInteractiveImport({
    downloadId,
    seriesId,
    seasonNumber,
    folder,
    filterExistingFiles,
  });

  const { sortKey, sortDirection, importMode } = useInteractiveImportOptions();

  const { updateInteractiveImportItem } = useUpdateInteractiveImportItem();
  const { updateInteractiveImportItems } = useUpdateInteractiveImportItems();

  const { reprocessInteractiveImportItems } =
    useReprocessInteractiveImportItems();

  const items = data;

  const { isDeleting, deleteEpisodeFiles, deleteError } =
    useDeleteEpisodeFiles();

  const { updateEpisodeFiles } = useUpdateEpisodeFiles();

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
  const [interactiveImportErrorMessage, setInteractiveImportErrorMessage] =
    useState<string | null>(null);
  const previousIsDeleting = usePrevious(isDeleting);
  const executeCommand = useExecuteCommand();

  const {
    allSelected,
    allUnselected,
    selectAll,
    unselectAll,
    toggleSelected,
    useSelectedIds,
  } = useSelect<InteractiveImport>();

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

  const selectedIds = useSelectedIds();

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

    const options: SelectInputOption[] = [
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
      {
        key: 'releaseType',
        value: translate('SelectReleaseType'),
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
        const sortDirection: SortDirection =
          (initialSortDirection as SortDirection) || 'ascending';

        setInteractiveImportSort({
          sortKey: initialSortKey,
          sortDirection,
        });
      }
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    []
  );

  useEffect(() => {
    if (!isDeleting && previousIsDeleting && !deleteError) {
      onModalClose();
    }
  }, [previousIsDeleting, isDeleting, deleteError, onModalClose]);

  const handleSelectAllChange = useCallback(
    ({ value }: CheckInputChanged) => {
      if (value) {
        selectAll();
      } else {
        unselectAll();
      }
    },
    [selectAll, unselectAll]
  );

  const handleSelectedChange = useCallback<OnSelectedChangeCallback>(
    ({ id, value, hasEpisodeFileId, shiftKey = false }) => {
      toggleSelected({
        id,
        isSelected: value,
        shiftKey,
      });

      setWithoutEpisodeFileIdRowsSelected(
        hasEpisodeFileId || !value
          ? without(withoutEpisodeFileIdRowsSelected, id as number)
          : [...withoutEpisodeFileIdRowsSelected, id as number]
      );
    },
    [
      withoutEpisodeFileIdRowsSelected,
      setWithoutEpisodeFileIdRowsSelected,
      toggleSelected,
    ]
  );

  const handleValidRowChange = useCallback(
    (id: number, isValid: boolean) => {
      if (isValid && invalidRowsSelected.includes(id)) {
        setInvalidRowsSelected(without(invalidRowsSelected, id));
      } else if (!isValid && !invalidRowsSelected.includes(id)) {
        setInvalidRowsSelected([...invalidRowsSelected, id]);
      }
    },
    [invalidRowsSelected, setInvalidRowsSelected]
  );

  const handleDeleteSelectedPress = useCallback(() => {
    setIsConfirmDeleteModalOpen(true);
  }, [setIsConfirmDeleteModalOpen]);

  const handleConfirmDelete = useCallback(() => {
    setIsConfirmDeleteModalOpen(false);

    const episodeFileIds = items.reduce((acc: number[], item) => {
      if (selectedIds.indexOf(item.id) > -1 && item.episodeFileId) {
        acc.push(item.episodeFileId);
      }

      return acc;
    }, []);

    deleteEpisodeFiles({ episodeFileIds });
  }, [items, selectedIds, setIsConfirmDeleteModalOpen, deleteEpisodeFiles]);

  const handleConfirmDeleteModalClose = useCallback(() => {
    setIsConfirmDeleteModalOpen(false);
  }, [setIsConfirmDeleteModalOpen]);

  const handleImportSelectedPress = useCallback(() => {
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
          releaseType,
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
              releaseType,
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
          releaseType,
          downloadId,
          episodeFileId,
        });
      }
    });

    let shouldClose = false;

    if (existingFiles.length) {
      updateEpisodeFiles(existingFiles);

      shouldClose = true;
    }

    if (files.length) {
      executeCommand({
        name: CommandNames.ManualImport,
        files,
        importMode: finalImportMode,
      });

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
    executeCommand,
    updateEpisodeFiles,
  ]);

  const handleSetInteractiveImportMode = useCallback(
    ({ importMode }: { importMode: ImportMode }) => {
      setInteractiveImportOption('importMode', importMode);
    },
    []
  );

  const handleSortPress = useCallback<SortCallback>(
    (sortKey, sortDirection) => {
      setInteractiveImportSort({ sortKey, sortDirection });
    },
    []
  );

  const handleFilterExistingFilesChange = useCallback(
    (value: string | undefined) => {
      const filter = value !== 'all';
      filterExistingFilesStore.setState(filter);
    },
    []
  );

  const handleImportModeChange = useCallback<
    ({ value }: { value: ImportMode }) => void
  >(
    ({ value }) => {
      handleSetInteractiveImportMode({ importMode: value });
    },
    [handleSetInteractiveImportMode]
  );

  const handleSelectModalSelect = useCallback<
    ({ value }: { value: SelectType }) => void
  >(
    ({ value }) => {
      setSelectModalOpen(value);
    },
    [setSelectModalOpen]
  );

  const handleSelectModalClose = useCallback(() => {
    setSelectModalOpen(null);
  }, [setSelectModalOpen]);

  const handleReprocessItems = useCallback(
    (ids: number[]) => {
      setReprocessingItems((prev) => {
        const newSet = new Set(prev);

        ids.forEach((id) => newSet.add(id));

        return newSet;
      });

      reprocessInteractiveImportItems(ids);
    },
    [reprocessInteractiveImportItems]
  );

  const handleSeriesSelect = useCallback(
    (series: Series) => {
      const updates = {
        series,
        seasonNumber: undefined,
        episodes: [],
      };

      updateInteractiveImportItems(selectedIds, updates);

      handleReprocessItems(selectedIds);
      setSelectModalOpen(null);
    },
    [
      selectedIds,
      updateInteractiveImportItems,
      setSelectModalOpen,
      handleReprocessItems,
    ]
  );

  const handleSeasonSelect = useCallback(
    (seasonNumber: number) => {
      const updates = {
        seasonNumber,
        episodes: [],
      };

      updateInteractiveImportItems(selectedIds, updates);
      handleReprocessItems(selectedIds);

      setSelectModalOpen(null);
    },
    [
      selectedIds,
      setSelectModalOpen,
      updateInteractiveImportItems,
      handleReprocessItems,
    ]
  );

  const handleEpisodesSelect = useCallback(
    (selectedEpisodes: SelectedEpisode[]) => {
      selectedEpisodes.forEach(({ id, episodes }) => {
        updateInteractiveImportItem(id, { episodes });
      });

      const selectedIds = selectedEpisodes.map(({ id }) => id);
      handleReprocessItems(selectedIds);
      setSelectModalOpen(null);
    },
    [updateInteractiveImportItem, setSelectModalOpen, handleReprocessItems]
  );

  const handleReleaseGroupSelect = useCallback(
    (releaseGroup: string) => {
      updateInteractiveImportItems(selectedIds, { releaseGroup });

      handleReprocessItems(selectedIds);
      setSelectModalOpen(null);
    },
    [
      selectedIds,
      updateInteractiveImportItems,
      setSelectModalOpen,
      handleReprocessItems,
    ]
  );

  const handleLanguagesSelect = useCallback(
    (newLanguages: Language[]) => {
      updateInteractiveImportItems(selectedIds, { languages: newLanguages });

      handleReprocessItems(selectedIds);
      setSelectModalOpen(null);
    },
    [
      selectedIds,
      updateInteractiveImportItems,
      setSelectModalOpen,
      handleReprocessItems,
    ]
  );

  const handleQualitySelect = useCallback(
    (quality: QualityModel) => {
      updateInteractiveImportItems(selectedIds, { quality });

      handleReprocessItems(selectedIds);
      setSelectModalOpen(null);
    },
    [
      selectedIds,
      updateInteractiveImportItems,
      setSelectModalOpen,
      handleReprocessItems,
    ]
  );

  const handleIndexerFlagsSelect = useCallback(
    (indexerFlags: number) => {
      updateInteractiveImportItems(selectedIds, { indexerFlags });

      handleReprocessItems(selectedIds);
      setSelectModalOpen(null);
    },
    [
      selectedIds,
      updateInteractiveImportItems,
      setSelectModalOpen,
      handleReprocessItems,
    ]
  );

  const handleReleaseTypeSelect = useCallback(
    (releaseType: string) => {
      updateInteractiveImportItems(selectedIds, {
        releaseType: releaseType as ReleaseType,
      });

      handleReprocessItems(selectedIds);
      setSelectModalOpen(null);
    },
    [
      selectedIds,
      updateInteractiveImportItems,
      setSelectModalOpen,
      handleReprocessItems,
    ]
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
        {showFilterExistingFiles ? (
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
                  name="all"
                  isSelected={!filterExistingFiles}
                  onPress={handleFilterExistingFilesChange}
                >
                  {translate('AllFiles')}
                </SelectedMenuItem>

                <SelectedMenuItem
                  name="new"
                  isSelected={filterExistingFiles}
                  onPress={handleFilterExistingFilesChange}
                >
                  {translate('UnmappedFilesOnly')}
                </SelectedMenuItem>
              </MenuContent>
            </Menu>
          </div>
        ) : null}

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
            onSortPress={handleSortPress}
            onSelectAllChange={handleSelectAllChange}
          >
            <TableBody>
              {items.map((item) => {
                return (
                  <InteractiveImportRow
                    key={item.id}
                    {...item}
                    allowSeriesChange={allowSeriesChange}
                    columns={columns}
                    modalTitle={modalTitle}
                    isReprocessing={reprocessingItems.has(item.id)}
                    onReprocessItems={handleReprocessItems}
                    onSelectedChange={handleSelectedChange}
                    onValidRowChange={handleValidRowChange}
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
              onPress={handleDeleteSelectedPress}
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
              onChange={handleImportModeChange}
            />
          ) : null}

          <SelectInput
            className={styles.bulkSelect}
            name="select"
            value="select"
            values={bulkSelectOptions}
            isDisabled={!selectedIds.length}
            onChange={handleSelectModalSelect}
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
            onPress={handleImportSelectedPress}
          >
            {folder ? translate('Apply') : translate('Import')}
          </Button>
        </div>
      </ModalFooter>

      <SelectSeriesModal
        isOpen={selectModalOpen === 'series'}
        modalTitle={modalTitle}
        onSeriesSelect={handleSeriesSelect}
        onModalClose={handleSelectModalClose}
      />

      <SelectSeasonModal
        isOpen={selectModalOpen === 'season'}
        seriesId={selectedItem?.series?.id}
        modalTitle={modalTitle}
        onSeasonSelect={handleSeasonSelect}
        onModalClose={handleSelectModalClose}
      />

      <SelectEpisodeModal
        isOpen={selectModalOpen === 'episode'}
        selectedIds={orderedSelectedIds}
        seriesId={selectedItem?.series?.id}
        seasonNumber={selectedItem?.seasonNumber}
        isAnime={selectedItem?.series?.seriesType === 'anime'}
        modalTitle={modalTitle}
        onEpisodesSelect={handleEpisodesSelect}
        onModalClose={handleSelectModalClose}
      />

      <SelectReleaseGroupModal
        isOpen={selectModalOpen === 'releaseGroup'}
        releaseGroup=""
        modalTitle={modalTitle}
        onReleaseGroupSelect={handleReleaseGroupSelect}
        onModalClose={handleSelectModalClose}
      />

      <SelectLanguageModal
        isOpen={selectModalOpen === 'language'}
        languageIds={[0]}
        modalTitle={modalTitle}
        onLanguagesSelect={handleLanguagesSelect}
        onModalClose={handleSelectModalClose}
      />

      <SelectQualityModal
        isOpen={selectModalOpen === 'quality'}
        qualityId={0}
        proper={false}
        real={false}
        modalTitle={modalTitle}
        onQualitySelect={handleQualitySelect}
        onModalClose={handleSelectModalClose}
      />

      <SelectIndexerFlagsModal
        isOpen={selectModalOpen === 'indexerFlags'}
        indexerFlags={0}
        modalTitle={modalTitle}
        onIndexerFlagsSelect={handleIndexerFlagsSelect}
        onModalClose={handleSelectModalClose}
      />

      <SelectReleaseTypeModal
        isOpen={selectModalOpen === 'releaseType'}
        releaseType="unknown"
        modalTitle={modalTitle}
        onReleaseTypeSelect={handleReleaseTypeSelect}
        onModalClose={handleSelectModalClose}
      />

      <ConfirmModal
        isOpen={isConfirmDeleteModalOpen}
        kind={kinds.DANGER}
        title={translate('DeleteSelectedEpisodeFiles')}
        message={translate('DeleteSelectedEpisodeFilesHelpText')}
        confirmLabel={translate('Delete')}
        onConfirm={handleConfirmDelete}
        onCancel={handleConfirmDeleteModalClose}
      />
    </ModalContent>
  );
}

function InteractiveImportModalContent(
  props: InteractiveImportModalContentProps
) {
  const filterExistingFiles = filterExistingFilesStore((state) => state);

  const { downloadId, seriesId, seasonNumber, folder } = props;
  const { data } = useInteractiveImport({
    downloadId,
    seriesId,
    seasonNumber,
    folder,
    filterExistingFiles,
  });

  return (
    <SelectProvider<InteractiveImport> items={data}>
      <InteractiveImportModalContentInner {...props} />
    </SelectProvider>
  );
}

export default InteractiveImportModalContent;
