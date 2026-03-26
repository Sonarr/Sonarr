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
  IndexerModel,
  useBulkDeleteIndexers,
  useBulkEditIndexers,
  useIndexersData,
  useSortedIndexers,
} from 'Settings/Indexers/useIndexers';
import {
  setManageIndexersSort,
  useManageIndexersOptions,
} from 'Settings/Indexers/useManageIndexersOptionsStore';
import { CheckInputChanged } from 'typings/inputs';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import translate from 'Utilities/String/translate';
import ManageIndexersEditModal from './Edit/ManageIndexersEditModal';
import ManageIndexersModalRow from './ManageIndexersModalRow';
import TagsModal from './Tags/TagsModal';
import styles from './ManageIndexersModalContent.css';

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
    name: 'enableRss',
    label: () => translate('EnableRss'),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'enableAutomaticSearch',
    label: () => translate('EnableAutomaticSearch'),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'enableInteractiveSearch',
    label: () => translate('EnableInteractiveSearch'),
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
    name: 'seasonSearchMaximumSingleEpisodeAge',
    label: () => translate('MaximumSingleEpisodeAge'),
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

interface ManageIndexersModalContentProps {
  onModalClose(): void;
}

interface ManageIndexersModalContentInnerProps {
  onModalClose(): void;
}

function ManageIndexersModalContentInner(
  props: ManageIndexersModalContentInnerProps
) {
  const { onModalClose } = props;

  const { sortKey, sortDirection } = useManageIndexersOptions();
  const { data, isFetching, isFetched, error } = useSortedIndexers();

  const { isDeleting, bulkDeleteIndexers } = useBulkDeleteIndexers();
  const { isSaving, bulkEditIndexers } = useBulkEditIndexers();

  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isTagsModalOpen, setIsTagsModalOpen] = useState(false);
  const [isSavingTags, setIsSavingTags] = useState(false);

  const {
    allSelected,
    allUnselected,
    anySelected,
    getSelectedIds,
    selectAll,
    unselectAll,
    useSelectedIds,
  } = useSelect<IndexerModel>();

  const onSortPress = useCallback((value: string) => {
    setManageIndexersSort({ sortKey: value });
  }, []);

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
    bulkDeleteIndexers({ ids: getSelectedIds() });
    setIsDeleteModalOpen(false);
  }, [bulkDeleteIndexers, getSelectedIds]);

  const onSavePress = useCallback(
    (payload: object) => {
      setIsEditModalOpen(false);

      bulkEditIndexers({
        ids: getSelectedIds(),
        ...payload,
      });
    },
    [getSelectedIds, bulkEditIndexers]
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

      bulkEditIndexers({
        ids: getSelectedIds(),
        tags,
        applyTags,
      });
    },
    [getSelectedIds, bulkEditIndexers]
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

  const selectedIds = useSelectedIds();
  const errorMessage = getErrorMessage(error, 'Unable to load indexers.');

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('ManageIndexers')}</ModalHeader>
      <ModalBody>
        {isFetching ? <LoadingIndicator /> : null}

        {error ? <div>{errorMessage}</div> : null}

        {isFetched && !error && !data.length ? (
          <Alert kind={kinds.INFO}>{translate('NoIndexersFound')}</Alert>
        ) : null}

        {isFetched && !!data.length && !isFetching && !isFetching ? (
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
                  <ManageIndexersModalRow
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

      <ManageIndexersEditModal
        isOpen={isEditModalOpen}
        indexerIds={selectedIds}
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
        title={translate('DeleteSelectedIndexers')}
        message={translate('DeleteSelectedIndexersMessageText', {
          count: selectedIds.length,
        })}
        confirmLabel={translate('Delete')}
        onConfirm={onConfirmDelete}
        onCancel={onDeleteModalClose}
      />
    </ModalContent>
  );
}

function ManageIndexersModalContent(props: ManageIndexersModalContentProps) {
  const items = useIndexersData();

  return (
    <SelectProvider items={items}>
      <ManageIndexersModalContentInner {...props} />
    </SelectProvider>
  );
}

export default ManageIndexersModalContent;
