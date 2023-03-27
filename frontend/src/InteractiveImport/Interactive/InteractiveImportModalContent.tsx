import { cloneDeep, without } from 'lodash';
import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
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
import usePrevious from 'Helpers/Hooks/usePrevious';
import useSelectState from 'Helpers/Hooks/useSelectState';
import { align, icons, kinds, scrollDirections } from 'Helpers/Props';
import SelectEpisodeModal from 'InteractiveImport/Episode/SelectEpisodeModal';
import ImportMode from 'InteractiveImport/ImportMode';
import SelectLanguageModal from 'InteractiveImport/Language/SelectLanguageModal';
import SelectQualityModal from 'InteractiveImport/Quality/SelectQualityModal';
import SelectReleaseGroupModal from 'InteractiveImport/ReleaseGroup/SelectReleaseGroupModal';
import SelectSeasonModal from 'InteractiveImport/Season/SelectSeasonModal';
import SelectSeriesModal from 'InteractiveImport/Series/SelectSeriesModal';
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
  updateInteractiveImportItems,
} from 'Store/Actions/interactiveImportActions';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import hasDifferentItems from 'Utilities/Object/hasDifferentItems';
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
  | 'language';

const COLUMNS = [
  {
    name: 'relativePath',
    label: 'Relative Path',
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'series',
    label: 'Series',
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'season',
    label: 'Season',
    isVisible: true,
  },
  {
    name: 'episodes',
    label: 'Episode(s)',
    isVisible: true,
  },
  {
    name: 'releaseGroup',
    label: 'Release Group',
    isVisible: true,
  },
  {
    name: 'quality',
    label: 'Quality',
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'languages',
    label: 'Languages',
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'size',
    label: 'Size',
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'customFormats',
    label: React.createElement(Icon, {
      name: icons.INTERACTIVE,
      title: 'Custom Format',
    }),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'rejections',
    label: React.createElement(Icon, {
      name: icons.DANGER,
      kind: kinds.DANGER,
    }),
    isSortable: true,
    isVisible: true,
  },
];

const filterExistingFilesOptions = {
  ALL: 'all',
  NEW: 'new',
};

const importModeOptions = [
  { key: 'chooseImportMode', value: 'Choose Import Mode', disabled: true },
  { key: 'move', value: 'Move Files' },
  { key: 'copy', value: 'Hardlink/Copy Files' },
];

function isSameEpisodeFile(file, originalFile) {
  const { series, seasonNumber, episodes } = file;

  if (!originalFile) {
    return false;
  }

  if (!originalFile.series || series.id !== originalFile.series.id) {
    return false;
  }

  if (seasonNumber !== originalFile.seasonNumber) {
    return false;
  }

  return !hasDifferentItems(originalFile.episodes, episodes);
}

const episodeFilesInfoSelector = createSelector(
  (state) => state.episodeFiles.isDeleting,
  (state) => state.episodeFiles.deleteError,
  (isDeleting, deleteError) => {
    return {
      isDeleting,
      deleteError,
    };
  }
);

const importModeSelector = createSelector(
  (state) => state.interactiveImport.importMode,
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
  autoSelectRow?: boolean;
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
    autoSelectRow = true,
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
  } = useSelector(createClientSideCollectionSelector('interactiveImport'));

  const { isDeleting, deleteError } = useSelector(episodeFilesInfoSelector);
  const importMode = useSelector(importModeSelector);

  const [invalidRowsSelected, setInvalidRowsSelected] = useState([]);
  const [
    withoutEpisodeFileIdRowsSelected,
    setWithoutEpisodeFileIdRowsSelected,
  ] = useState([]);
  const [selectModalOpen, setSelectModalOpen] = useState<SelectType | null>(
    null
  );
  const [isConfirmDeleteModalOpen, setIsConfirmDeleteModalOpen] =
    useState(false);
  const [filterExistingFiles, setFilterExistingFiles] = useState(false);
  const [interactiveImportErrorMessage, setInteractiveImportErrorMessage] =
    useState<string | null>(null);
  const [selectState, setSelectState] = useSelectState();
  const [bulkSelectOptions, setBulkSelectOptions] = useState([
    { key: 'select', value: 'Select...', disabled: true },
    { key: 'season', value: 'Select Season' },
    { key: 'episode', value: 'Select Episode(s)' },
    { key: 'quality', value: 'Select Quality' },
    { key: 'releaseGroup', value: 'Select Release Group' },
    { key: 'language', value: 'Select Language' },
  ]);
  const { allSelected, allUnselected, selectedState } = selectState;
  const previousIsDeleting = usePrevious(isDeleting);
  const dispatch = useDispatch();

  const columns: Column[] = useMemo(() => {
    const result = cloneDeep(COLUMNS);

    if (!showSeries) {
      result.find((c) => c.name === 'series').isVisible = false;
    }

    return result;
  }, [showSeries]);

  const selectedIds = useMemo(() => {
    return getSelectedIds(selectedState);
  }, [selectedState]);

  useEffect(
    () => {
      if (allowSeriesChange) {
        const newBulkSelectOptions = [...bulkSelectOptions];

        newBulkSelectOptions.splice(1, 0, {
          key: 'series',
          value: 'Select Series',
        });

        setBulkSelectOptions(newBulkSelectOptions);
      }

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
    ({ value }) => {
      setSelectState({ type: value ? 'selectAll' : 'unselectAll', items });
    },
    [items, setSelectState]
  );

  const onSelectedChange = useCallback(
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

    const episodeFileIds = items.reduce((acc, item) => {
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
    const finalImportMode =
      downloadId || !showImportMode ? ImportMode.Auto : importMode;

    const existingFiles = [];
    const files = [];

    if (finalImportMode === 'chooseImportMode') {
      setInteractiveImportErrorMessage('An import mode must be selected');

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
          episodeFileId,
        } = item;

        if (!series) {
          setInteractiveImportErrorMessage(
            'Series must be chosen for each selected file'
          );
          return;
        }

        if (isNaN(seasonNumber)) {
          setInteractiveImportErrorMessage(
            'Season must be chosen for each selected file'
          );
          return;
        }

        if (!episodes || !episodes.length) {
          setInteractiveImportErrorMessage(
            'One or more episodes must be chosen for each selected file'
          );
          return;
        }

        if (!quality) {
          setInteractiveImportErrorMessage(
            'Quality must be chosen for each selected file'
          );
          return;
        }

        if (!languages) {
          setInteractiveImportErrorMessage(
            'Language(s) must be chosen for each selected file'
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
          importMode,
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

  const onSortPress = useCallback(
    (sortKey, sortDirection) => {
      dispatch(setInteractiveImportSort({ sortKey, sortDirection }));
    },
    [dispatch]
  );

  const onFilterExistingFilesChange = useCallback(
    (value) => {
      const filter = value !== filterExistingFilesOptions.ALL;

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

  const onImportModeChange = useCallback(
    ({ value }) => {
      dispatch(setInteractiveImportMode({ importMode: value }));
    },
    [dispatch]
  );

  const onSelectModalSelect = useCallback(
    ({ value }) => {
      setSelectModalOpen(value);
    },
    [setSelectModalOpen]
  );

  const onSelectModalClose = useCallback(() => {
    setSelectModalOpen(null);
  }, [setSelectModalOpen]);

  const onSeriesSelect = useCallback(
    (series) => {
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
    (seasonNumber) => {
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
    (episodes) => {
      dispatch(
        updateInteractiveImportItems({
          ids: selectedIds,
          episodes,
        })
      );

      dispatch(reprocessInteractiveImportItems({ ids: selectedIds }));

      setSelectModalOpen(null);
    },
    [selectedIds, setSelectModalOpen, dispatch]
  );

  const onReleaseGroupSelect = useCallback(
    (releaseGroup) => {
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
    (newLanguages) => {
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
    (quality) => {
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

  const orderedSelectedIds = items.reduce((acc, file) => {
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
    'Unable to load manual import items'
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
                  {filterExistingFiles ? 'Unmapped Files Only' : 'All Files'}
                </div>
              </MenuButton>

              <MenuContent>
                <SelectedMenuItem
                  name={filterExistingFilesOptions.ALL}
                  isSelected={!filterExistingFiles}
                  onPress={onFilterExistingFilesChange}
                >
                  All Files
                </SelectedMenuItem>

                <SelectedMenuItem
                  name={filterExistingFilesOptions.NEW}
                  isSelected={filterExistingFiles}
                  onPress={onFilterExistingFilesChange}
                >
                  Unmapped Files Only
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
                    autoSelectRow={autoSelectRow}
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
          ? 'No video files were found in the selected folder'
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
              Delete
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
            Import
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
        isAnime={selectedItem?.series.type === 'anime'}
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

      <ConfirmModal
        isOpen={isConfirmDeleteModalOpen}
        kind={kinds.DANGER}
        title="Delete Selected Episode Files"
        message={'Are you sure you want to delete the selected episode files?'}
        confirmLabel="Delete"
        onConfirm={onConfirmDelete}
        onCancel={onConfirmDeleteModalClose}
      />
    </ModalContent>
  );
}

export default InteractiveImportModalContent;
