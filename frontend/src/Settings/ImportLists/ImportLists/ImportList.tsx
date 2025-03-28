import React, { useCallback, useState } from 'react';
import { useDispatch } from 'react-redux';
import Card from 'Components/Card';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import TagList from 'Components/TagList';
import { icons, kinds } from 'Helpers/Props';
import { deleteImportList } from 'Store/Actions/settingsActions';
import useTags from 'Tags/useTags';
import formatShortTimeSpan from 'Utilities/Date/formatShortTimeSpan';
import translate from 'Utilities/String/translate';
import EditImportListModal from './EditImportListModal';
import styles from './ImportList.css';

interface ImportListProps {
  id: number;
  name: string;
  enableAutomaticAdd: boolean;
  tags: number[];
  minRefreshInterval: string;
  onCloneImportListPress: (id: number) => void;
}

function ImportList({
  id,
  name,
  enableAutomaticAdd,
  tags,
  minRefreshInterval,
  onCloneImportListPress,
}: ImportListProps) {
  const dispatch = useDispatch();
  const tagList = useTags();

  const [isEditImportListModalOpen, setIsEditImportListModalOpen] =
    useState(false);

  const [isDeleteImportListModalOpen, setIsDeleteImportListModalOpen] =
    useState(false);

  const handleEditImportListPress = useCallback(() => {
    setIsEditImportListModalOpen(true);
  }, []);

  const handleEditImportListModalClose = useCallback(() => {
    setIsEditImportListModalOpen(false);
  }, []);

  const handleDeleteImportListPress = useCallback(() => {
    setIsEditImportListModalOpen(false);
    setIsDeleteImportListModalOpen(true);
  }, []);

  const handleDeleteImportListModalClose = useCallback(() => {
    setIsDeleteImportListModalOpen(false);
  }, []);

  const handleConfirmDeleteImportList = useCallback(() => {
    dispatch(deleteImportList({ id }));
  }, [id, dispatch]);

  const handleCloneImportListPress = useCallback(() => {
    onCloneImportListPress(id);
  }, [id, onCloneImportListPress]);

  return (
    <Card
      className={styles.list}
      overlayContent={true}
      onPress={handleEditImportListPress}
    >
      <div className={styles.nameContainer}>
        <div className={styles.name}>{name}</div>

        <IconButton
          className={styles.cloneButton}
          title={translate('CloneImportList')}
          name={icons.CLONE}
          onPress={handleCloneImportListPress}
        />
      </div>

      <div className={styles.enabled}>
        {enableAutomaticAdd ? (
          <Label kind={kinds.SUCCESS}>{translate('AutomaticAdd')}</Label>
        ) : null}
      </div>

      <TagList tags={tags} tagList={tagList} />

      <div className={styles.enabled}>
        <Label kind={kinds.DEFAULT} title="List Refresh Interval">
          {`${translate('Refresh')}: ${formatShortTimeSpan(
            minRefreshInterval
          )}`}
        </Label>
      </div>

      <EditImportListModal
        id={id}
        isOpen={isEditImportListModalOpen}
        onModalClose={handleEditImportListModalClose}
        onDeleteImportListPress={handleDeleteImportListPress}
      />

      <ConfirmModal
        isOpen={isDeleteImportListModalOpen}
        kind={kinds.DANGER}
        title={translate('DeleteImportList')}
        message={translate('DeleteImportListMessageText', { name })}
        confirmLabel={translate('Delete')}
        onConfirm={handleConfirmDeleteImportList}
        onCancel={handleDeleteImportListModalClose}
      />
    </Card>
  );
}

export default ImportList;
