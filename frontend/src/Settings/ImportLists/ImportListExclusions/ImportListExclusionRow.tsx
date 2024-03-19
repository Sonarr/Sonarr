import React, { useCallback } from 'react';
import IconButton from 'Components/Link/IconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import useModalOpenState from 'Helpers/Hooks/useModalOpenState';
import { icons, kinds } from 'Helpers/Props';
import ImportListExclusion from 'typings/ImportListExclusion';
import translate from 'Utilities/String/translate';
import EditImportListExclusionModal from './EditImportListExclusionModal';
import styles from './ImportListExclusionRow.css';

interface ImportListExclusionRowProps extends ImportListExclusion {
  onConfirmDeleteImportListExclusion: (id: number) => void;
}

function ImportListExclusionRow(props: ImportListExclusionRowProps) {
  const { id, title, tvdbId, onConfirmDeleteImportListExclusion } = props;

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

  const onConfirmDeleteImportListExclusionPress = useCallback(() => {
    onConfirmDeleteImportListExclusion(id);
  }, [id, onConfirmDeleteImportListExclusion]);

  return (
    <TableRow>
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
        onConfirm={onConfirmDeleteImportListExclusionPress}
        onCancel={setDeleteImportListExclusionModalClosed}
      />
    </TableRow>
  );
}

export default ImportListExclusionRow;
