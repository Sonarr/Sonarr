import React, { useCallback, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { SelectProvider, useSelect } from 'App/Select/SelectContext';
import { ImportListAppState } from 'App/State/SettingsAppState';
import Alert from 'Components/Alert';
import Button from 'Components/Link/Button';
import SpinnerButton from 'Components/Link/SpinnerButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { kinds } from 'Helpers/Props';
import {
  bulkDeleteImportLists,
  bulkEditImportLists,
} from 'Store/Actions/settingsActions';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import ImportList from 'typings/ImportList';
import { CheckInputChanged } from 'typings/inputs';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import translate from 'Utilities/String/translate';
import ManageImportListsEditModal from './Edit/ManageImportListsEditModal';
import ManageImportListsModalRow from './ManageImportListsModalRow';
import TagsModal from './Tags/TagsModal';
import styles from './ManageImportListsModalContent.css';

const COLUMNS = [
  {
    name: 'name',
    label: () => translate('Name'),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'implementation',
    label: () => translate('Implementation'),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'qualityProfileId',
    label: () => translate('QualityProfile'),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'rootFolderPath',
    label: () => translate('RootFolder'),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'enableAutomaticAdd',
    label: () => translate('AutoAdd'),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'tags',
    label: () => translate('Tags'),
    isSortable: true,
    isVisible: true,
  },
];

interface ManageImportListsModalContentProps {
  onModalClose(): void;
}

interface ManageImportListsModalContentInnerProps {
  onModalClose(): void;
}

function ManageImportListsModalContentInner(
  props: ManageImportListsModalContentInnerProps
) {
  const { onModalClose } = props;

  const {
    isFetching,
    isPopulated,
    isDeleting,
    isSaving,
    error,
    items,
  }: ImportListAppState = useSelector(
    createClientSideCollectionSelector('settings.importLists')
  );
  const dispatch = useDispatch();

  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isTagsModalOpen, setIsTagsModalOpen] = useState(false);
  const [isSavingTags, setIsSavingTags] = useState(false);

  const {
    allSelected,
    allUnselected,
    anySelected,
    selectAll,
    unselectAll,
    useSelectedIds,
  } = useSelect<ImportList>();

  const selectedIds = useSelectedIds();

  const onDeletePress = useCallback(() => {
    setIsDeleteModalOpen(true);
  }, [setIsDeleteModalOpen]);

  const onDeleteModalClose = useCallback(() => {
    setIsDeleteModalOpen(false);
  }, [setIsDeleteModalOpen]);

  const onEditPress = useCallback(() => {
    setIsEditModalOpen(true);
  }, [setIsEditModalOpen]);

  const onEditModalClose = useCallback(() => {
    setIsEditModalOpen(false);
  }, [setIsEditModalOpen]);

  const onConfirmDelete = useCallback(() => {
    dispatch(bulkDeleteImportLists({ ids: selectedIds }));
    setIsDeleteModalOpen(false);
  }, [selectedIds, dispatch]);

  const onSavePress = useCallback(
    (payload: object) => {
      setIsEditModalOpen(false);

      dispatch(
        bulkEditImportLists({
          ids: selectedIds,
          ...payload,
        })
      );
    },
    [selectedIds, dispatch]
  );

  const onTagsPress = useCallback(() => {
    setIsTagsModalOpen(true);
  }, [setIsTagsModalOpen]);

  const onTagsModalClose = useCallback(() => {
    setIsTagsModalOpen(false);
  }, [setIsTagsModalOpen]);

  const onApplyTagsPress = useCallback(
    (tags: number[], applyTags: string) => {
      setIsSavingTags(true);
      setIsTagsModalOpen(false);

      dispatch(
        bulkEditImportLists({
          ids: selectedIds,
          tags,
          applyTags,
        })
      );
    },
    [selectedIds, dispatch]
  );

  const onSelectAllChange = useCallback(
    ({ value }: CheckInputChanged) => {
      if (value) {
        selectAll();
      } else {
        unselectAll();
      }
    },
    [selectAll, unselectAll]
  );

  const errorMessage = getErrorMessage(error, 'Unable to load import lists.');

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('ManageImportLists')}</ModalHeader>
      <ModalBody>
        {isFetching ? <LoadingIndicator /> : null}

        {error ? <div>{errorMessage}</div> : null}

        {isPopulated && !error && !items.length ? (
          <Alert kind={kinds.INFO}>{translate('NoImportListsFound')}</Alert>
        ) : null}

        {isPopulated && !!items.length && !isFetching && !isFetching ? (
          <Table
            columns={COLUMNS}
            horizontalScroll={true}
            selectAll={true}
            allSelected={allSelected}
            allUnselected={allUnselected}
            onSelectAllChange={onSelectAllChange}
          >
            <TableBody>
              {items.map((item) => {
                return (
                  <ManageImportListsModalRow
                    key={item.id}
                    {...item}
                    columns={COLUMNS}
                  />
                );
              })}
            </TableBody>
          </Table>
        ) : null}
      </ModalBody>

      <ModalFooter>
        <div className={styles.leftButtons}>
          <SpinnerButton
            kind={kinds.DANGER}
            isSpinning={isDeleting}
            isDisabled={!anySelected}
            onPress={onDeletePress}
          >
            {translate('Delete')}
          </SpinnerButton>

          <SpinnerButton
            isSpinning={isSaving}
            isDisabled={!anySelected}
            onPress={onEditPress}
          >
            {translate('Edit')}
          </SpinnerButton>

          <SpinnerButton
            isSpinning={isSaving && isSavingTags}
            isDisabled={!anySelected}
            onPress={onTagsPress}
          >
            {translate('SetTags')}
          </SpinnerButton>
        </div>

        <Button onPress={onModalClose}>{translate('Close')}</Button>
      </ModalFooter>

      <ManageImportListsEditModal
        isOpen={isEditModalOpen}
        importListIds={selectedIds}
        onModalClose={onEditModalClose}
        onSavePress={onSavePress}
      />

      <TagsModal
        isOpen={isTagsModalOpen}
        ids={selectedIds}
        onApplyTagsPress={onApplyTagsPress}
        onModalClose={onTagsModalClose}
      />

      <ConfirmModal
        isOpen={isDeleteModalOpen}
        kind={kinds.DANGER}
        title={translate('DeleteSelectedImportLists')}
        message={translate('DeleteSelectedImportListsMessageText', {
          count: selectedIds.length,
        })}
        confirmLabel={translate('Delete')}
        onConfirm={onConfirmDelete}
        onCancel={onDeleteModalClose}
      />
    </ModalContent>
  );
}

function ManageImportListsModalContent(
  props: ManageImportListsModalContentProps
) {
  const { items }: ImportListAppState = useSelector(
    createClientSideCollectionSelector('settings.importLists')
  );

  return (
    <SelectProvider items={items}>
      <ManageImportListsModalContentInner {...props} />
    </SelectProvider>
  );
}

export default ManageImportListsModalContent;
