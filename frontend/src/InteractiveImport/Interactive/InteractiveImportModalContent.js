import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
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
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { align, icons, kinds, scrollDirections } from 'Helpers/Props';
import SelectEpisodeModal from 'InteractiveImport/Episode/SelectEpisodeModal';
import SelectLanguageModal from 'InteractiveImport/Language/SelectLanguageModal';
import SelectQualityModal from 'InteractiveImport/Quality/SelectQualityModal';
import SelectReleaseGroupModal from 'InteractiveImport/ReleaseGroup/SelectReleaseGroupModal';
import SelectSeasonModal from 'InteractiveImport/Season/SelectSeasonModal';
import SelectSeriesModal from 'InteractiveImport/Series/SelectSeriesModal';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
import selectAll from 'Utilities/Table/selectAll';
import toggleSelected from 'Utilities/Table/toggleSelected';
import InteractiveImportRow from './InteractiveImportRow';
import styles from './InteractiveImportModalContent.css';

const columns = [
  {
    name: 'relativePath',
    label: 'Relative Path',
    isSortable: true,
    isVisible: true
  },
  {
    name: 'series',
    label: 'Series',
    isSortable: true,
    isVisible: true
  },
  {
    name: 'season',
    label: 'Season',
    isVisible: true
  },
  {
    name: 'episodes',
    label: 'Episode(s)',
    isVisible: true
  },
  {
    name: 'releaseGroup',
    label: 'Release Group',
    isVisible: true
  },
  {
    name: 'quality',
    label: 'Quality',
    isSortable: true,
    isVisible: true
  },
  {
    name: 'languages',
    label: 'Languages',
    isSortable: true,
    isVisible: true
  },
  {
    name: 'size',
    label: 'Size',
    isSortable: true,
    isVisible: true
  },
  {
    name: 'rejections',
    label: React.createElement(Icon, {
      name: icons.DANGER,
      kind: kinds.DANGER
    }),
    isSortable: true,
    isVisible: true
  }
];

const filterExistingFilesOptions = {
  ALL: 'all',
  NEW: 'new'
};

const importModeOptions = [
  { key: 'chooseImportMode', value: 'Choose Import Mode', disabled: true },
  { key: 'move', value: 'Move Files' },
  { key: 'copy', value: 'Hardlink/Copy Files' }
];

const SELECT = 'select';
const SERIES = 'series';
const SEASON = 'season';
const EPISODE = 'episode';
const RELEASE_GROUP = 'releaseGroup';
const QUALITY = 'quality';
const LANGUAGE = 'language';

class InteractiveImportModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    const instanceColumns = _.cloneDeep(columns);

    if (!props.showSeries) {
      instanceColumns.find((c) => c.name === 'series').isVisible = false;
    }

    this.state = {
      allSelected: false,
      allUnselected: false,
      lastToggled: null,
      selectedState: {},
      invalidRowsSelected: [],
      withoutEpisodeFileIdRowsSelected: [],
      selectModalOpen: null,
      columns: instanceColumns,
      isConfirmDeleteModalOpen: false
    };
  }

  componentDidUpdate(prevProps) {
    const {
      isDeleting,
      deleteError,
      onModalClose
    } = this.props;

    if (!isDeleting && prevProps.isDeleting && !deleteError) {
      onModalClose();
    }
  }

  //
  // Control

  getSelectedIds = () => {
    return getSelectedIds(this.state.selectedState);
  };

  //
  // Listeners

  onSelectAllChange = ({ value }) => {
    this.setState(selectAll(this.state.selectedState, value));
  };

  onSelectedChange = ({ id, value, hasEpisodeFileId, shiftKey = false }) => {
    this.setState((state) => {
      return {
        ...toggleSelected(state, this.props.items, id, value, shiftKey),
        withoutEpisodeFileIdRowsSelected: hasEpisodeFileId || !value ?
          _.without(state.withoutEpisodeFileIdRowsSelected, id) :
          [...state.withoutEpisodeFileIdRowsSelected, id]
      };
    });
  };

  onValidRowChange = (id, isValid) => {
    this.setState((state) => {
      if (isValid) {
        return {
          invalidRowsSelected: _.without(state.invalidRowsSelected, id)
        };
      }

      return {
        invalidRowsSelected: [...state.invalidRowsSelected, id]
      };
    });
  };

  onDeleteSelectedPress = () => {
    this.setState({ isConfirmDeleteModalOpen: true });
  };

  onConfirmDelete = () => {
    this.setState({ isConfirmDeleteModalOpen: false });
    this.props.onDeleteSelectedPress(this.getSelectedIds());
  };

  onConfirmDeleteModalClose = () => {
    this.setState({ isConfirmDeleteModalOpen: false });
  };

  onImportSelectedPress = () => {
    const {
      downloadId,
      showImportMode,
      importMode,
      onImportSelectedPress
    } = this.props;

    const selected = this.getSelectedIds();
    const finalImportMode = downloadId || !showImportMode ? 'auto' : importMode;

    onImportSelectedPress(selected, finalImportMode);
  };

  onFilterExistingFilesChange = (value) => {
    this.props.onFilterExistingFilesChange(value !== filterExistingFilesOptions.ALL);
  };

  onImportModeChange = ({ value }) => {
    this.props.onImportModeChange(value);
  };

  onSelectModalSelect = ({ value }) => {
    this.setState({ selectModalOpen: value });
  };

  onSelectModalClose = () => {
    this.setState({ selectModalOpen: null });
  };

  //
  // Render

  render() {
    const {
      downloadId,
      allowSeriesChange,
      autoSelectRow,
      showFilterExistingFiles,
      showDelete,
      showImportMode,
      filterExistingFiles,
      title,
      folder,
      isFetching,
      isPopulated,
      error,
      items,
      sortKey,
      sortDirection,
      importMode,
      interactiveImportErrorMessage,
      isDeleting,
      modalTitle,
      onSortPress,
      onModalClose
    } = this.props;

    const {
      allSelected,
      allUnselected,
      selectedState,
      invalidRowsSelected,
      withoutEpisodeFileIdRowsSelected,
      selectModalOpen,
      isConfirmDeleteModalOpen
    } = this.state;

    const selectedIds = this.getSelectedIds();

    const orderedSelectedIds = items.reduce((acc, file) => {
      if (selectedIds.includes(file.id)) {
        acc.push(file.id);
      }

      return acc;
    }, []);

    const selectedItem = selectedIds.length ?
      items.find((file) => file.id === selectedIds[0]) :
      null;

    const errorMessage = getErrorMessage(error, 'Unable to load manual import items');

    const bulkSelectOptions = [
      { key: SELECT, value: 'Select...', disabled: true },
      { key: SEASON, value: 'Select Season' },
      { key: EPISODE, value: 'Select Episode(s)' },
      { key: QUALITY, value: 'Select Quality' },
      { key: RELEASE_GROUP, value: 'Select Release Group' },
      { key: LANGUAGE, value: 'Select Language' }
    ];

    if (allowSeriesChange) {
      bulkSelectOptions.splice(1, 0, {
        key: SERIES,
        value: 'Select Series'
      });
    }

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          {modalTitle} - {title || folder}
        </ModalHeader>

        <ModalBody scrollDirection={scrollDirections.BOTH}>
          {
            showFilterExistingFiles &&
              <div className={styles.filterContainer}>
                <Menu alignMenu={align.RIGHT}>
                  <MenuButton>
                    <Icon
                      name={icons.FILTER}
                      size={22}
                    />

                    <div className={styles.filterText}>
                      {
                        filterExistingFiles ? 'Unmapped Files Only' : 'All Files'
                      }
                    </div>
                  </MenuButton>

                  <MenuContent>
                    <SelectedMenuItem
                      name={filterExistingFilesOptions.ALL}
                      isSelected={!filterExistingFiles}
                      onPress={this.onFilterExistingFilesChange}
                    >
                      All Files
                    </SelectedMenuItem>

                    <SelectedMenuItem
                      name={filterExistingFilesOptions.NEW}
                      isSelected={filterExistingFiles}
                      onPress={this.onFilterExistingFilesChange}
                    >
                      Unmapped Files Only
                    </SelectedMenuItem>
                  </MenuContent>
                </Menu>
              </div>
          }

          {
            isFetching &&
              <LoadingIndicator />
          }

          {
            error &&
              <div>{errorMessage}</div>
          }

          {
            isPopulated && !!items.length && !isFetching && !isFetching &&
              <Table
                columns={this.state.columns}
                horizontalScroll={true}
                selectAll={true}
                allSelected={allSelected}
                allUnselected={allUnselected}
                sortKey={sortKey}
                sortDirection={sortDirection}
                onSortPress={onSortPress}
                onSelectAllChange={this.onSelectAllChange}
              >
                <TableBody>
                  {
                    items.map((item) => {
                      return (
                        <InteractiveImportRow
                          key={item.id}
                          isSelected={selectedState[item.id]}
                          {...item}
                          allowSeriesChange={allowSeriesChange}
                          autoSelectRow={autoSelectRow}
                          columns={this.state.columns}
                          modalTitle={modalTitle}
                          onSelectedChange={this.onSelectedChange}
                          onValidRowChange={this.onValidRowChange}
                        />
                      );
                    })
                  }
                </TableBody>
              </Table>
          }

          {
            isPopulated && !items.length && !isFetching &&
              'No video files were found in the selected folder'
          }
        </ModalBody>

        <ModalFooter className={styles.footer}>
          <div className={styles.leftButtons}>
            {
              showDelete ?
                <SpinnerButton
                  className={styles.deleteButton}
                  kind={kinds.DANGER}
                  isSpinning={isDeleting}
                  isDisabled={!selectedIds.length || !!withoutEpisodeFileIdRowsSelected.length}
                  onPress={this.onDeleteSelectedPress}
                >
                  Delete
                </SpinnerButton> :
                null
            }

            {
              !downloadId && showImportMode ?
                <SelectInput
                  className={styles.importMode}
                  name="importMode"
                  value={importMode}
                  values={importModeOptions}
                  onChange={this.onImportModeChange}
                /> :
                null
            }

            <SelectInput
              className={styles.bulkSelect}
              name="select"
              value={SELECT}
              values={bulkSelectOptions}
              isDisabled={!selectedIds.length}
              onChange={this.onSelectModalSelect}
            />
          </div>

          <div className={styles.rightButtons}>
            <Button onPress={onModalClose}>
              Cancel
            </Button>

            {
              interactiveImportErrorMessage &&
                <span className={styles.errorMessage}>{interactiveImportErrorMessage}</span>
            }

            <Button
              kind={kinds.SUCCESS}
              isDisabled={!selectedIds.length || !!invalidRowsSelected.length}
              onPress={this.onImportSelectedPress}
            >
              Import
            </Button>
          </div>
        </ModalFooter>

        <SelectSeriesModal
          isOpen={selectModalOpen === SERIES}
          ids={selectedIds}
          modalTitle={modalTitle}
          onModalClose={this.onSelectModalClose}
        />

        <SelectSeasonModal
          isOpen={selectModalOpen === SEASON}
          ids={selectedIds}
          seriesId={selectedItem && selectedItem.series && selectedItem.series.id}
          modalTitle={modalTitle}
          onModalClose={this.onSelectModalClose}
        />

        <SelectEpisodeModal
          isOpen={selectModalOpen === EPISODE}
          ids={orderedSelectedIds}
          seriesId={selectedItem && selectedItem.series && selectedItem.series.id}
          seasonNumber={selectedItem && selectedItem.seasonNumber}
          modalTitle={modalTitle}
          onModalClose={this.onSelectModalClose}
        />

        <SelectReleaseGroupModal
          isOpen={selectModalOpen === RELEASE_GROUP}
          ids={selectedIds}
          releaseGroup=""
          modalTitle={modalTitle}
          onModalClose={this.onSelectModalClose}
        />

        <SelectLanguageModal
          isOpen={selectModalOpen === LANGUAGE}
          ids={selectedIds}
          languageIds={[0]}
          modalTitle={modalTitle}
          onModalClose={this.onSelectModalClose}
        />

        <SelectQualityModal
          isOpen={selectModalOpen === QUALITY}
          ids={selectedIds}
          qualityId={0}
          proper={false}
          real={false}
          modalTitle={modalTitle}
          onModalClose={this.onSelectModalClose}
        />

        <ConfirmModal
          isOpen={isConfirmDeleteModalOpen}
          kind={kinds.DANGER}
          title="Delete Selected Episode Files"
          message={'Are you sure you want to delete the selected episode files?'}
          confirmLabel="Delete"
          onConfirm={this.onConfirmDelete}
          onCancel={this.onConfirmDeleteModalClose}
        />
      </ModalContent>
    );
  }
}

InteractiveImportModalContent.propTypes = {
  downloadId: PropTypes.string,
  showSeries: PropTypes.bool.isRequired,
  allowSeriesChange: PropTypes.bool.isRequired,
  autoSelectRow: PropTypes.bool.isRequired,
  showDelete: PropTypes.bool.isRequired,
  showImportMode: PropTypes.bool.isRequired,
  showFilterExistingFiles: PropTypes.bool.isRequired,
  filterExistingFiles: PropTypes.bool.isRequired,
  importMode: PropTypes.string.isRequired,
  title: PropTypes.string,
  folder: PropTypes.string,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  sortKey: PropTypes.string,
  sortDirection: PropTypes.string,
  interactiveImportErrorMessage: PropTypes.string,
  isDeleting: PropTypes.bool.isRequired,
  deleteError: PropTypes.object,
  modalTitle: PropTypes.string.isRequired,
  onSortPress: PropTypes.func.isRequired,
  onFilterExistingFilesChange: PropTypes.func.isRequired,
  onImportModeChange: PropTypes.func.isRequired,
  onDeleteSelectedPress: PropTypes.func.isRequired,
  onImportSelectedPress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

InteractiveImportModalContent.defaultProps = {
  showSeries: true,
  allowSeriesChange: true,
  autoSelectRow: true,
  showFilterExistingFiles: false,
  showDelete: false,
  showImportMode: true,
  importMode: 'move'
};

export default InteractiveImportModalContent;
