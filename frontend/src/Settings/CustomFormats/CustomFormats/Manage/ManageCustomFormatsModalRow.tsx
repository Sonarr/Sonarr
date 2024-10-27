import React, { useCallback, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import IconButton from 'Components/Link/IconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import Column from 'Components/Table/Column';
import TableRow from 'Components/Table/TableRow';
import { icons } from 'Helpers/Props';
import { deleteCustomFormat } from 'Store/Actions/settingsActions';
import { SelectStateInputProps } from 'typings/props';
import translate from 'Utilities/String/translate';
import EditCustomFormatModalConnector from '../EditCustomFormatModalConnector';
import styles from './ManageCustomFormatsModalRow.css';

interface ManageCustomFormatsModalRowProps {
  id: number;
  name: string;
  includeCustomFormatWhenRenaming: boolean;
  columns: Column[];
  isSelected?: boolean;
  onSelectedChange(result: SelectStateInputProps): void;
}

function isDeletingSelector() {
  return createSelector(
    (state: AppState) => state.settings.customFormats.isDeleting,
    (isDeleting) => {
      return isDeleting;
    }
  );
}

function ManageCustomFormatsModalRow(props: ManageCustomFormatsModalRowProps) {
  const {
    id,
    isSelected,
    name,
    includeCustomFormatWhenRenaming,
    onSelectedChange,
  } = props;

  const dispatch = useDispatch();
  const isDeleting = useSelector(isDeletingSelector());

  const [isEditCustomFormatModalOpen, setIsEditCustomFormatModalOpen] =
    useState(false);

  const [isDeleteCustomFormatModalOpen, setIsDeleteCustomFormatModalOpen] =
    useState(false);

  const handlelectedChange = useCallback(
    (result: SelectStateInputProps) => {
      onSelectedChange({
        ...result,
      });
    },
    [onSelectedChange]
  );

  const handleEditCustomFormatModalOpen = useCallback(() => {
    setIsEditCustomFormatModalOpen(true);
  }, [setIsEditCustomFormatModalOpen]);

  const handleEditCustomFormatModalClose = useCallback(() => {
    setIsEditCustomFormatModalOpen(false);
  }, [setIsEditCustomFormatModalOpen]);

  const handleDeleteCustomFormatPress = useCallback(() => {
    setIsEditCustomFormatModalOpen(false);
    setIsDeleteCustomFormatModalOpen(true);
  }, [setIsEditCustomFormatModalOpen, setIsDeleteCustomFormatModalOpen]);

  const handleDeleteCustomFormatModalClose = useCallback(() => {
    setIsDeleteCustomFormatModalOpen(false);
  }, [setIsDeleteCustomFormatModalOpen]);

  const handleConfirmDeleteCustomFormat = useCallback(() => {
    dispatch(deleteCustomFormat({ id }));
  }, [id, dispatch]);

  return (
    <TableRow>
      <TableSelectCell
        id={id}
        isSelected={isSelected}
        onSelectedChange={handlelectedChange}
      />

      <TableRowCell className={styles.name}>{name}</TableRowCell>

      <TableRowCell className={styles.includeCustomFormatWhenRenaming}>
        {includeCustomFormatWhenRenaming ? translate('Yes') : translate('No')}
      </TableRowCell>

      <TableRowCell className={styles.actions}>
        <IconButton
          name={icons.EDIT}
          onPress={handleEditCustomFormatModalOpen}
        />
      </TableRowCell>

      <EditCustomFormatModalConnector
        id={id}
        isOpen={isEditCustomFormatModalOpen}
        onModalClose={handleEditCustomFormatModalClose}
        onDeleteCustomFormatPress={handleDeleteCustomFormatPress}
      />

      <ConfirmModal
        isOpen={isDeleteCustomFormatModalOpen}
        kind="danger"
        title={translate('DeleteCustomFormat')}
        message={translate('DeleteCustomFormatMessageText', { name })}
        confirmLabel={translate('Delete')}
        isSpinning={isDeleting}
        onConfirm={handleConfirmDeleteCustomFormat}
        onCancel={handleDeleteCustomFormatModalClose}
      />
    </TableRow>
  );
}

export default ManageCustomFormatsModalRow;
