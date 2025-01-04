import React, { useCallback, useState } from 'react';
import { useDispatch } from 'react-redux';
import Card from 'Components/Card';
import Label from 'Components/Label';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import TagList from 'Components/TagList';
import { kinds } from 'Helpers/Props';
import { deleteDownloadClient } from 'Store/Actions/settingsActions';
import useTags from 'Tags/useTags';
import translate from 'Utilities/String/translate';
import EditDownloadClientModal from './EditDownloadClientModal';
import styles from './DownloadClient.css';

interface DownloadClientProps {
  id: number;
  name: string;
  enable: boolean;
  priority: number;
  tags: number[];
}

function DownloadClient({
  id,
  name,
  enable,
  priority,
  tags,
}: DownloadClientProps) {
  const dispatch = useDispatch();
  const tagList = useTags();

  const [isEditDownloadClientModalOpen, setIsEditDownloadClientModalOpen] =
    useState(false);

  const [isDeleteDownloadClientModalOpen, setIsDeleteDownloadClientModalOpen] =
    useState(false);

  const handleEditDownloadClientPress = useCallback(() => {
    setIsEditDownloadClientModalOpen(true);
  }, []);

  const handleEditDownloadClientModalClose = useCallback(() => {
    setIsEditDownloadClientModalOpen(false);
  }, []);

  const handleDeleteDownloadClientPress = useCallback(() => {
    setIsEditDownloadClientModalOpen(false);
    setIsDeleteDownloadClientModalOpen(true);
  }, []);

  const handleDeleteDownloadClientModalClose = useCallback(() => {
    setIsDeleteDownloadClientModalOpen(false);
  }, []);

  const handleConfirmDeleteDownloadClient = useCallback(() => {
    dispatch(deleteDownloadClient({ id }));
  }, [id, dispatch]);

  return (
    <Card
      className={styles.downloadClient}
      overlayContent={true}
      onPress={handleEditDownloadClientPress}
    >
      <div className={styles.name}>{name}</div>

      <div className={styles.enabled}>
        {enable ? (
          <Label kind={kinds.SUCCESS}>{translate('Enabled')}</Label>
        ) : (
          <Label kind={kinds.DISABLED} outline={true}>
            {translate('Disabled')}
          </Label>
        )}

        {priority > 1 ? (
          <Label kind={kinds.DISABLED} outline={true}>
            {translate('PrioritySettings', { priority })}
          </Label>
        ) : null}
      </div>

      <TagList tags={tags} tagList={tagList} />

      <EditDownloadClientModal
        id={id}
        isOpen={isEditDownloadClientModalOpen}
        onModalClose={handleEditDownloadClientModalClose}
        onDeleteDownloadClientPress={handleDeleteDownloadClientPress}
      />

      <ConfirmModal
        isOpen={isDeleteDownloadClientModalOpen}
        kind={kinds.DANGER}
        title={translate('DeleteDownloadClient')}
        message={translate('DeleteDownloadClientMessageText', { name })}
        confirmLabel={translate('Delete')}
        onConfirm={handleConfirmDeleteDownloadClient}
        onCancel={handleDeleteDownloadClientModalClose}
      />
    </Card>
  );
}

export default DownloadClient;
