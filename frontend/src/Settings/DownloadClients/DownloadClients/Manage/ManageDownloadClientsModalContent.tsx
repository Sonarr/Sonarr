import React, { useCallback, useState } from 'react';
import { SelectProvider, useSelect } from 'App/Select/SelectContext';
import Alert from 'Components/Alert';
import Button from 'Components/Link/Button';
import SpinnerButton from 'Components/Link/SpinnerButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import Column from 'Components/Table/Column';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { kinds } from 'Helpers/Props';
import {
  DownloadClientModel,
  useBulkDeleteDownloadClients,
  useBulkEditDownloadClients,
  useDownloadClientsData,
  useSortedDownloadClients,
} from 'Settings/DownloadClients/DownloadClients/useDownloadClients';
import {
  setManageDownloadClientsSort,
  useManageDownloadClientsOptions,
} from 'Settings/DownloadClients/useManageDownloadClientsOptionsStore';
import { CheckInputChanged } from 'typings/inputs';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import translate from 'Utilities/String/translate';
import ManageDownloadClientsEditModal from './Edit/ManageDownloadClientsEditModal';
import ManageDownloadClientsModalRow from './ManageDownloadClientsModalRow';
import TagsModal from './Tags/TagsModal';
import styles from './ManageDownloadClientsModalContent.css';

const COLUMNS: Column[] = [
  {
    name: 'name',
    label: () => translate('Name'),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'protocol',
    label: () => translate('Protocol'),
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
    name: 'enable',
    label: () => translate('Enabled'),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'priority',
    label: () => translate('Priority'),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'removeCompletedDownloads',
    label: () => translate('RemoveCompleted'),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'removeFailedDownloads',
    label: () => translate('RemoveFailed'),
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

interface ManageDownloadClientsModalContentProps {
  onModalClose(): void;
}

function ManageDownloadClientsModalContentInner({
  onModalClose,
}: ManageDownloadClientsModalContentProps) {
  const { sortKey, sortDirection } = useManageDownloadClientsOptions();
  const { data, isFetching, isFetched, error } = useSortedDownloadClients(
    sortKey,
    sortDirection
  );

  const { isDeleting, bulkDeleteDownloadClients } =
    useBulkDeleteDownloadClients();
  const { isSaving, bulkEditDownloadClients } = useBulkEditDownloadClients();

  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isTagsModalOpen, setIsTagsModalOpen] = useState(false);
  const [isSavingTags, setIsSavingTags] = useState(false);

  const {
    allSelected,
    allUnselected,
    anySelected,
    selectedCount,
    getSelectedIds,
    selectAll,
    unselectAll,
    useSelectedIds,
  } = useSelect<DownloadClientModel>();

  const selectedIds = useSelectedIds();

  const onSortPress = useCallback((value: string) => {
    setManageDownloadClientsSort({ sortKey: value });
  }, []);

  const onDeletePress = useCallback(() => {
    setIsDeleteModalOpen(true);
  }, []);

  const onDeleteModalClose = useCallback(() => {
    setIsDeleteModalOpen(false);
  }, []);

  const onEditPress = useCallback(() => {
    setIsEditModalOpen(true);
  }, []);

  const onEditModalClose = useCallback(() => {
    setIsEditModalOpen(false);
  }, []);

  const onConfirmDelete = useCallback(() => {
    bulkDeleteDownloadClients({ ids: getSelectedIds() });
    setIsDeleteModalOpen(false);
  }, [bulkDeleteDownloadClients, getSelectedIds]);

  const onSavePress = useCallback(
    (payload: object) => {
      setIsEditModalOpen(false);

      bulkEditDownloadClients({
        ids: getSelectedIds(),
        ...payload,
      });
    },
    [getSelectedIds, bulkEditDownloadClients]
  );

  const onTagsPress = useCallback(() => {
    setIsTagsModalOpen(true);
  }, []);

  const onTagsModalClose = useCallback(() => {
    setIsTagsModalOpen(false);
  }, []);

  const onApplyTagsPress = useCallback(
    (tags: number[], applyTags: string) => {
      setIsSavingTags(true);
      setIsTagsModalOpen(false);

      bulkEditDownloadClients({
        ids: getSelectedIds(),
        tags,
        applyTags,
      });
    },
    [getSelectedIds, bulkEditDownloadClients]
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

  const errorMessage = getErrorMessage(
    error,
    'Unable to load download clients.'
  );

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('ManageDownloadClients')}</ModalHeader>
      <ModalBody>
        {isFetching ? <LoadingIndicator /> : null}

        {error ? <div>{errorMessage}</div> : null}

        {isFetched && !error && !data.length ? (
          <Alert kind={kinds.INFO}>{translate('NoDownloadClientsFound')}</Alert>
        ) : null}

        {isFetched && !!data.length && !isFetching ? (
          <Table
            columns={COLUMNS}
            horizontalScroll={true}
            selectAll={true}
            allSelected={allSelected}
            allUnselected={allUnselected}
            sortKey={sortKey}
            sortDirection={sortDirection}
            onSelectAllChange={onSelectAllChange}
            onSortPress={onSortPress}
          >
            <TableBody>
              {data.map((item) => {
                return (
                  <ManageDownloadClientsModalRow
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

      <ManageDownloadClientsEditModal
        isOpen={isEditModalOpen}
        downloadClientIds={selectedIds}
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
        title={translate('DeleteSelectedDownloadClients')}
        message={translate('DeleteSelectedDownloadClientsMessageText', {
          count: selectedCount,
        })}
        confirmLabel={translate('Delete')}
        onConfirm={onConfirmDelete}
        onCancel={onDeleteModalClose}
      />
    </ModalContent>
  );
}

function ManageDownloadClientsModalContent(
  props: ManageDownloadClientsModalContentProps
) {
  const items = useDownloadClientsData();

  return (
    <SelectProvider items={items}>
      <ManageDownloadClientsModalContentInner {...props} />
    </SelectProvider>
  );
}

export default ManageDownloadClientsModalContent;
