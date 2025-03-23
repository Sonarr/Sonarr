import React, { useCallback, useMemo, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { CustomFormatAppState } from 'App/State/SettingsAppState';
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
import useSelectState from 'Helpers/Hooks/useSelectState';
import { kinds } from 'Helpers/Props';
import {
  bulkDeleteCustomFormats,
  bulkEditCustomFormats,
  setManageCustomFormatsSort,
} from 'Store/Actions/settingsActions';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import { CheckInputChanged } from 'typings/inputs';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import translate from 'Utilities/String/translate';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
import ManageCustomFormatsEditModal from './Edit/ManageCustomFormatsEditModal';
import ManageCustomFormatsModalRow from './ManageCustomFormatsModalRow';
import styles from './ManageCustomFormatsModalContent.css';

// TODO: This feels janky to do, but not sure of a better way currently
type OnSelectedChangeCallback = React.ComponentProps<
  typeof ManageCustomFormatsModalRow
>['onSelectedChange'];

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
  onModalClose(): void;
}

function ManageCustomFormatsModalContent(
  props: ManageCustomFormatsModalContentProps
) {
  const { onModalClose } = props;

  const {
    isFetching,
    isPopulated,
    isDeleting,
    isSaving,
    error,
    items,
    sortKey,
    sortDirection,
  }: CustomFormatAppState = useSelector(
    createClientSideCollectionSelector('settings.customFormats')
  );
  const dispatch = useDispatch();

  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);

  const [selectState, setSelectState] = useSelectState();

  const { allSelected, allUnselected, selectedState } = selectState;

  const selectedIds: number[] = useMemo(() => {
    return getSelectedIds(selectedState);
  }, [selectedState]);

  const selectedCount = selectedIds.length;

  const onSortPress = useCallback(
    (value: string) => {
      dispatch(setManageCustomFormatsSort({ sortKey: value }));
    },
    [dispatch]
  );

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
    dispatch(bulkDeleteCustomFormats({ ids: selectedIds }));
    setIsDeleteModalOpen(false);
  }, [selectedIds, dispatch]);

  const onSavePress = useCallback(
    (payload: object) => {
      setIsEditModalOpen(false);

      dispatch(
        bulkEditCustomFormats({
          ids: selectedIds,
          ...payload,
        })
      );
    },
    [selectedIds, dispatch]
  );

  const onSelectAllChange = useCallback(
    ({ value }: CheckInputChanged) => {
      setSelectState({ type: value ? 'selectAll' : 'unselectAll', items });
    },
    [items, setSelectState]
  );

  const onSelectedChange = useCallback<OnSelectedChangeCallback>(
    ({ id, value, shiftKey = false }) => {
      setSelectState({
        type: 'toggleSelected',
        items,
        id,
        isSelected: value,
        shiftKey,
      });
    },
    [items, setSelectState]
  );

  const errorMessage = getErrorMessage(error, 'Unable to load custom formats.');
  const anySelected = selectedCount > 0;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('ManageCustomFormats')}</ModalHeader>
      <ModalBody>
        {isFetching ? <LoadingIndicator /> : null}

        {error ? <div>{errorMessage}</div> : null}

        {isPopulated && !error && !items.length ? (
          <Alert kind={kinds.INFO}>{translate('NoCustomFormatsFound')}</Alert>
        ) : null}

        {isPopulated && !!items.length && !isFetching && !isFetching ? (
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
              {items.map((item) => {
                return (
                  <ManageCustomFormatsModalRow
                    key={item.id}
                    isSelected={selectedState[item.id]}
                    {...item}
                    columns={COLUMNS}
                    onSelectedChange={onSelectedChange}
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
          count: selectedIds.length,
        })}
        confirmLabel={translate('Delete')}
        onConfirm={onConfirmDelete}
        onCancel={onDeleteModalClose}
      />
    </ModalContent>
  );
}

export default ManageCustomFormatsModalContent;
