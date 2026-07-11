import React, { useCallback, useState } from 'react';
import Card from 'Components/Card';
import Label from 'Components/Label';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import TagList from 'Components/TagList';
import { kinds, sizes } from 'Helpers/Props';
import { useTagList } from 'Tags/useTags';
import translate from 'Utilities/String/translate';
import { NotificationModel, useDeleteConnection } from '../useConnections';
import EditNotificationModal from './EditNotificationModal';
import styles from './Notification.css';

function Notification({
  id,
  name,
  onGrab,
  onDownload,
  onUpgrade,
  onImportComplete,
  onRename,
  onSeriesAdd,
  onSeriesDelete,
  onEpisodeFileDelete,
  onEpisodeFileDeleteForUpgrade,
  onHealthIssue,
  onHealthRestored,
  onApplicationUpdate,
  onManualInteractionRequired,
  supportsOnGrab,
  supportsOnDownload,
  supportsOnUpgrade,
  supportsOnImportComplete,
  supportsOnRename,
  supportsOnSeriesAdd,
  supportsOnSeriesDelete,
  supportsOnEpisodeFileDelete,
  supportsOnEpisodeFileDeleteForUpgrade,
  supportsOnHealthIssue,
  supportsOnHealthRestored,
  supportsOnApplicationUpdate,
  supportsOnManualInteractionRequired,
  tags,
}: NotificationModel) {
  const tagList = useTagList();
  const { deleteConnection } = useDeleteConnection(id);

  const [isEditNotificationModalOpen, setIsEditNotificationModalOpen] =
    useState(false);
  const [isDeleteNotificationModalOpen, setIsDeleteNotificationModalOpen] =
    useState(false);

  const handleEditNotificationPress = useCallback(() => {
    setIsEditNotificationModalOpen(true);
  }, []);

  const handleEditNotificationModalClose = useCallback(() => {
    setIsEditNotificationModalOpen(false);
  }, []);

  const handleDeleteNotificationPress = useCallback(() => {
    setIsEditNotificationModalOpen(false);
    setIsDeleteNotificationModalOpen(true);
  }, []);

  const handleDeleteNotificationModalClose = useCallback(() => {
    setIsDeleteNotificationModalOpen(false);
  }, []);

  const handleConfirmDeleteNotification = useCallback(() => {
    deleteConnection();
  }, [deleteConnection]);

  return (
    <Card
      className={styles.notification}
      overlayClassName={styles.overlay}
      overlayContent={true}
      aria-label={translate('EditConnectionName', { name })}
      onPress={handleEditNotificationPress}
    >
      <div className={styles.name}>{name}</div>

      <div className={styles.labels}>
        {supportsOnGrab && onGrab ? (
          <Label kind={kinds.SUCCESS} size={sizes.MEDIUM}>
            {translate('OnGrab')}
          </Label>
        ) : null}

        {supportsOnDownload && onDownload ? (
          <Label kind={kinds.SUCCESS} size={sizes.MEDIUM}>
            {translate('OnFileImport')}
          </Label>
        ) : null}

        {supportsOnUpgrade && onDownload && onUpgrade ? (
          <Label kind={kinds.SUCCESS} size={sizes.MEDIUM}>
            {translate('OnFileUpgrade')}
          </Label>
        ) : null}

        {supportsOnImportComplete && onImportComplete ? (
          <Label kind={kinds.SUCCESS} size={sizes.MEDIUM}>
            {translate('OnImportComplete')}
          </Label>
        ) : null}

        {supportsOnRename && onRename ? (
          <Label kind={kinds.SUCCESS} size={sizes.MEDIUM}>
            {translate('OnRename')}
          </Label>
        ) : null}

        {supportsOnHealthIssue && onHealthIssue ? (
          <Label kind={kinds.SUCCESS} size={sizes.MEDIUM}>
            {translate('OnHealthIssue')}
          </Label>
        ) : null}

        {supportsOnHealthRestored && onHealthRestored ? (
          <Label kind={kinds.SUCCESS} size={sizes.MEDIUM}>
            {translate('OnHealthRestored')}
          </Label>
        ) : null}

        {supportsOnApplicationUpdate && onApplicationUpdate ? (
          <Label kind={kinds.SUCCESS} size={sizes.MEDIUM}>
            {translate('OnApplicationUpdate')}
          </Label>
        ) : null}

        {supportsOnSeriesAdd && onSeriesAdd ? (
          <Label kind={kinds.SUCCESS} size={sizes.MEDIUM}>
            {translate('OnSeriesAdd')}
          </Label>
        ) : null}

        {supportsOnSeriesDelete && onSeriesDelete ? (
          <Label kind={kinds.SUCCESS} size={sizes.MEDIUM}>
            {translate('OnSeriesDelete')}
          </Label>
        ) : null}

        {supportsOnEpisodeFileDelete && onEpisodeFileDelete ? (
          <Label kind={kinds.SUCCESS} size={sizes.MEDIUM}>
            {translate('OnEpisodeFileDelete')}
          </Label>
        ) : null}

        {supportsOnEpisodeFileDeleteForUpgrade &&
        onEpisodeFileDelete &&
        onEpisodeFileDeleteForUpgrade ? (
          <Label kind={kinds.SUCCESS} size={sizes.MEDIUM}>
            {translate('OnEpisodeFileDeleteForUpgrade')}
          </Label>
        ) : null}

        {supportsOnManualInteractionRequired && onManualInteractionRequired ? (
          <Label kind={kinds.SUCCESS} size={sizes.MEDIUM}>
            {translate('OnManualInteractionRequired')}
          </Label>
        ) : null}

        {!onGrab &&
        !onDownload &&
        !onRename &&
        !onImportComplete &&
        !onHealthIssue &&
        !onHealthRestored &&
        !onApplicationUpdate &&
        !onSeriesAdd &&
        !onSeriesDelete &&
        !onEpisodeFileDelete &&
        !onManualInteractionRequired ? (
          <Label kind={kinds.DISABLED} outline={true} size={sizes.MEDIUM}>
            {translate('Disabled')}
          </Label>
        ) : null}
      </div>

      <TagList tags={tags} tagList={tagList} />

      <EditNotificationModal
        id={id}
        isOpen={isEditNotificationModalOpen}
        onModalClose={handleEditNotificationModalClose}
        onDeleteNotificationPress={handleDeleteNotificationPress}
      />

      <ConfirmModal
        isOpen={isDeleteNotificationModalOpen}
        kind={kinds.DANGER}
        title={translate('DeleteNotification')}
        message={translate('DeleteNotificationMessageText', { name })}
        confirmLabel={translate('Delete')}
        onConfirm={handleConfirmDeleteNotification}
        onCancel={handleDeleteNotificationModalClose}
      />
    </Card>
  );
}

export default Notification;
