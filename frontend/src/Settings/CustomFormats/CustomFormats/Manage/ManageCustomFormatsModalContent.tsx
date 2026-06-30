import React, { useCallback, useMemo, useState } from 'react';
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
import { kinds, sortDirections } from 'Helpers/Props';
import { SortDirection } from 'Helpers/Props/sortDirections';
import { CheckInputChanged } from 'typings/inputs';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import translate from 'Utilities/String/translate';
import {
  CustomFormat,
  useBulkDeleteCustomFormats,
  useBulkEditCustomFormats,
  useCustomFormats,
} from '../useCustomFormats';
import ManageCustomFormatsEditModal from './Edit/ManageCustomFormatsEditModal';
import ManageCustomFormatsModalRow from './ManageCustomFormatsModalRow';
import styles from './ManageCustomFormatsModalContent.css';

const COLUMNS: Column[] = [
  {
    name: 'name',
    label: () => translate('Name'),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'includeCustomFormatWhenRenaming',
    label: () => translate('IncludeCustomFormatWhenRenaming'),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'actions',
    label: '',
    isVisible: true,
  },
];

interface ManageCustomFormatsModalContentProps {
  onModalClose: () => void;
}

interface ManageCustomFormatsModalContentInnerProps {
  items: ReadonlyArray<CustomFormat>;
  onModalClose: () => void;
}

function ManageCustomFormatsModalContentInner({
  items,
  onModalClose,
}: ManageCustomFormatsModalContentInnerProps) {
  const [sortKey, setSortKey] = useState<string>('name');
  const [sortDirection, setSortDirection] = useState<SortDirection>(
    sortDirections.ASCENDING
  );

  const sortedItems = useMemo(() => {
    const sorted = [...items].sort((a, b) => {
      if (sortKey === 'includeCustomFormatWhenRenaming') {
        return (
          Number(a.includeCustomFormatWhenRenaming) -
          Number(b.includeCustomFormatWhenRenaming)
        );
      }

      return a.name.localeCompare(b.name);
    });

    return sortDirection === sortDirections.DESCENDING
      ? sorted.reverse()
      : sorted;
  }, [items, sortKey, sortDirection]);

  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);

  const {
    allSelected,
    allUnselected,
    anySelected,
    selectedCount,
    getSelectedIds,
    selectAll,
    unselectAll,
    useSelectedIds,
  } = useSelect<CustomFormat>();

  const selectedIds = useSelectedIds();

  const { bulkEditCustomFormats, isSaving } = useBulkEditCustomFormats(() => {
    setIsEditModalOpen(false);
  });

  const { bulkDeleteCustomFormats, isDeleting } = useBulkDeleteCustomFormats(
    () => {
      setIsDeleteModalOpen(false);
    }
  );

  const onSortPress = useCallback(
    (value: string) => {
      if (value === sortKey) {
        setSortDirection((d) =>
          d === sortDirections.ASCENDING
            ? sortDirections.DESCENDING
            : sortDirections.ASCENDING
        );
        return;
      }

      setSortKey(value);
      setSortDirection(sortDirections.ASCENDING);
    },
    [sortKey]
  );

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
    bulkDeleteCustomFormats({ ids: getSelectedIds() });
  }, [bulkDeleteCustomFormats, getSelectedIds]);

  const onSavePress = useCallback(
    (payload: { includeCustomFormatWhenRenaming?: boolean }) => {
      bulkEditCustomFormats({
        ids: getSelectedIds(),
        ...payload,
      });
    },
    [bulkEditCustomFormats, getSelectedIds]
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

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('ManageCustomFormats')}</ModalHeader>
      <ModalBody>
        {sortedItems.length === 0 ? (
          <Alert kind={kinds.INFO}>{translate('NoCustomFormatsFound')}</Alert>
        ) : (
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
              {sortedItems.map((item) => (
                <ManageCustomFormatsModalRow
                  key={item.id}
                  {...item}
                  columns={COLUMNS}
                />
              ))}
            </TableBody>
          </Table>
        )}
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
        </div>

        <Button onPress={onModalClose}>{translate('Close')}</Button>
      </ModalFooter>

      <ManageCustomFormatsEditModal
        isOpen={isEditModalOpen}
        customFormatIds={selectedIds}
        onModalClose={onEditModalClose}
        onSavePress={onSavePress}
      />

      <ConfirmModal
        isOpen={isDeleteModalOpen}
        kind={kinds.DANGER}
        title={translate('DeleteSelectedCustomFormats')}
        message={translate('DeleteSelectedCustomFormatsMessageText', {
          count: selectedCount,
        })}
        confirmLabel={translate('Delete')}
        onConfirm={onConfirmDelete}
        onCancel={onDeleteModalClose}
      />
    </ModalContent>
  );
}

function ManageCustomFormatsModalContent({
  onModalClose,
}: ManageCustomFormatsModalContentProps) {
  const { data: items, isFetching, isFetched, error } = useCustomFormats();

  if (isFetching && !isFetched) {
    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>{translate('ManageCustomFormats')}</ModalHeader>
        <ModalBody>
          <LoadingIndicator />
        </ModalBody>
      </ModalContent>
    );
  }

  if (error) {
    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>{translate('ManageCustomFormats')}</ModalHeader>
        <ModalBody>
          {getErrorMessage(error, translate('CustomFormatsLoadError'))}
        </ModalBody>
      </ModalContent>
    );
  }

  return (
    <SelectProvider items={items}>
      <ManageCustomFormatsModalContentInner
        items={items}
        onModalClose={onModalClose}
      />
    </SelectProvider>
  );
}

export default ManageCustomFormatsModalContent;
