import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import IconButton from 'Components/Link/IconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import TableRow from 'Components/Table/TableRow';
import useModalOpenState from 'Helpers/Hooks/useModalOpenState';
import { icons, kinds } from 'Helpers/Props';
import { deleteImportListExclusion } from 'Store/Actions/Settings/importListExclusions';
import ImportListExclusion from 'typings/ImportListExclusion';
import { SelectStateInputProps } from 'typings/props';
import translate from 'Utilities/String/translate';
import EditImportListExclusionModal from './EditImportListExclusionModal';
import styles from './ImportListExclusionRow.css';

interface ImportListExclusionRowProps extends ImportListExclusion {
  isSelected: boolean;
  onSelectedChange: (options: SelectStateInputProps) => void;
}

function ImportListExclusionRow(props: ImportListExclusionRowProps) {
  const { id, tvdbId, title, isSelected, onSelectedChange } = props;

  const dispatch = useDispatch();

  const [
    isEditImportListExclusionModalOpen,
    setEditImportListExclusionModalOpen,
    setEditImportListExclusionModalClosed,
  ] = useModalOpenState(false);

  const [
    isDeleteImportListExclusionModalOpen,
    setDeleteImportListExclusionModalOpen,
    setDeleteImportListExclusionModalClosed,
  ] = useModalOpenState(false);

  const handleDeletePress = useCallback(() => {
    dispatch(deleteImportListExclusion({ id }));
  }, [id, dispatch]);

  return (
    <TableRow>
      <TableSelectCell
        id={id}
        isSelected={isSelected}
        onSelectedChange={onSelectedChange}
      />

      <TableRowCell>{title}</TableRowCell>
      <TableRowCell>{tvdbId}</TableRowCell>

      <TableRowCell className={styles.actions}>
        <IconButton
          name={icons.EDIT}
          onPress={setEditImportListExclusionModalOpen}
        />
      </TableRowCell>

      <EditImportListExclusionModal
        id={id}
        isOpen={isEditImportListExclusionModalOpen}
        onModalClose={setEditImportListExclusionModalClosed}
        onDeleteImportListExclusionPress={setDeleteImportListExclusionModalOpen}
      />

      <ConfirmModal
        isOpen={isDeleteImportListExclusionModalOpen}
        kind={kinds.DANGER}
        title={translate('DeleteImportListExclusion')}
        message={translate('DeleteImportListExclusionMessageText')}
        confirmLabel={translate('Delete')}
        onConfirm={handleDeletePress}
        onCancel={setDeleteImportListExclusionModalClosed}
      />
    </TableRow>
  );
}

export default ImportListExclusionRow;
