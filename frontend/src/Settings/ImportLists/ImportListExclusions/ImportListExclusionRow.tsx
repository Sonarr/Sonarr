import React, { useCallback, useState } from 'react';
import IconButton from 'Components/Link/IconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import { icons, kinds } from 'Helpers/Props';
import ImportListExclusion from 'typings/ImportListExclusion';
import translate from 'Utilities/String/translate';
import EditImportListExclusionModalConnector from './EditImportListExclusionModalConnector';
import styles from './ImportListExclusionRow.css';

interface ImportListExclusionRowProps extends ImportListExclusion {
  onConfirmDeleteImportListExclusion: (id: number) => void;
}

function ImportListExclusionRow(props: ImportListExclusionRowProps) {
  const { id, title, tvdbId, onConfirmDeleteImportListExclusion } = props;

  const [
    isEditImportListExclusionModalOpen,
    setIsEditImportListExclusionModalOpen,
  ] = useState(false);

  const onEditImportListExclusionPress = useCallback(
    () => setIsEditImportListExclusionModalOpen(true),
    [setIsEditImportListExclusionModalOpen]
  );

  const onEditImportListExclusionModalClose = useCallback(
    () => setIsEditImportListExclusionModalOpen(false),
    [setIsEditImportListExclusionModalOpen]
  );

  const [
    isDeleteImportListExclusionModalOpen,
    setIsDeleteImportListExclusionModalOpen,
  ] = useState(false);

  const onDeleteImportListExclusionPress = useCallback(
    () => setIsDeleteImportListExclusionModalOpen(true),
    [setIsDeleteImportListExclusionModalOpen]
  );

  const onDeleteImportListExclusionModalClose = useCallback(
    () => setIsDeleteImportListExclusionModalOpen(false),
    [setIsDeleteImportListExclusionModalOpen]
  );

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
          onPress={onEditImportListExclusionPress}
        />
      </TableRowCell>

      <EditImportListExclusionModalConnector
        id={id}
        isOpen={isEditImportListExclusionModalOpen}
        onModalClose={onEditImportListExclusionModalClose}
        onDeleteImportListExclusionPress={onDeleteImportListExclusionPress}
      />

      <ConfirmModal
        isOpen={isDeleteImportListExclusionModalOpen}
        kind={kinds.DANGER}
        title={translate('DeleteImportListExclusion')}
        message={translate('DeleteImportListExclusionMessageText')}
        confirmLabel={translate('Delete')}
        onConfirm={onConfirmDeleteImportListExclusionPress}
        onCancel={onDeleteImportListExclusionModalClose}
      />
    </TableRow>
  );
}

export default ImportListExclusionRow;
