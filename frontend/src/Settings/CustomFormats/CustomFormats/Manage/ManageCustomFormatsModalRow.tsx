import React, { useCallback, useState } from 'react';
import { useSelect } from 'App/Select/SelectContext';
import IconButton from 'Components/Link/IconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import Column from 'Components/Table/Column';
import TableRow from 'Components/Table/TableRow';
import { icons } from 'Helpers/Props';
import { SelectStateInputProps } from 'typings/props';
import translate from 'Utilities/String/translate';
import EditCustomFormatModal from '../EditCustomFormatModal';
import { CustomFormat, useDeleteCustomFormat } from '../useCustomFormats';
import styles from './ManageCustomFormatsModalRow.css';

interface ManageCustomFormatsModalRowProps {
  id: number;
  name: string;
  includeCustomFormatWhenRenaming: boolean;
  columns: Column[];
}

function ManageCustomFormatsModalRow({
  id,
  name,
  includeCustomFormatWhenRenaming,
}: ManageCustomFormatsModalRowProps) {
  const { toggleSelected, useIsSelected } = useSelect<CustomFormat>();
  const isSelected = useIsSelected(id);
  const { deleteCustomFormat, isDeleting } = useDeleteCustomFormat(id);

  const [isEditCustomFormatModalOpen, setIsEditCustomFormatModalOpen] =
    useState(false);

  const [isDeleteCustomFormatModalOpen, setIsDeleteCustomFormatModalOpen] =
    useState(false);

  const handleSelectedChange = useCallback(
    ({ id, value, shiftKey }: SelectStateInputProps) => {
      toggleSelected({ id, isSelected: value, shiftKey });
    },
    [toggleSelected]
  );

  const handleEditCustomFormatModalOpen = useCallback(() => {
    setIsEditCustomFormatModalOpen(true);
  }, []);

  const handleEditCustomFormatModalClose = useCallback(() => {
    setIsEditCustomFormatModalOpen(false);
  }, []);

  const handleDeleteCustomFormatPress = useCallback(() => {
    setIsEditCustomFormatModalOpen(false);
    setIsDeleteCustomFormatModalOpen(true);
  }, []);

  const handleDeleteCustomFormatModalClose = useCallback(() => {
    setIsDeleteCustomFormatModalOpen(false);
  }, []);

  const handleConfirmDeleteCustomFormat = useCallback(() => {
    deleteCustomFormat();
  }, [deleteCustomFormat]);

  return (
    <TableRow>
      <TableSelectCell
        id={id}
        isSelected={isSelected}
        onSelectedChange={handleSelectedChange}
      />

      <TableRowCell className={styles.name}>{name}</TableRowCell>

      <TableRowCell className={styles.includeCustomFormatWhenRenaming}>
        {includeCustomFormatWhenRenaming ? translate('Yes') : translate('No')}
      </TableRowCell>

      <TableRowCell className={styles.actions}>
        <IconButton
          name={icons.EDIT}
          aria-label={translate('Edit')}
          onPress={handleEditCustomFormatModalOpen}
        />
      </TableRowCell>

      <EditCustomFormatModal
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
