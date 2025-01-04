import React, { useCallback, useState } from 'react';
import { useDispatch } from 'react-redux';
import Card from 'Components/Card';
import Label from 'Components/Label';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import TagList from 'Components/TagList';
import { kinds } from 'Helpers/Props';
import { deleteNotification } from 'Store/Actions/settingsActions';
import useTags from 'Tags/useTags';
import NotificationModel from 'typings/Notification';
import translate from 'Utilities/String/translate';
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
  const dispatch = useDispatch();
  const tagList = useTags();

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
    dispatch(deleteNotification({ id }));
  }, [id, dispatch]);

  return (
    <Card
      className={styles.notification}
      overlayContent={true}
      onPress={handleEditNotificationPress}
    >
      <div className={styles.name}>{name}</div>

      {supportsOnGrab && onGrab ? (
        <Label kind={kinds.SUCCESS}>{translate('OnGrab')}</Label>
      ) : null}

      {supportsOnDownload && onDownload ? (
        <Label kind={kinds.SUCCESS}>{translate('OnFileImport')}</Label>
      ) : null}

      {supportsOnUpgrade && onDownload && onUpgrade ? (
        <Label kind={kinds.SUCCESS}>{translate('OnFileUpgrade')}</Label>
      ) : null}

      {supportsOnImportComplete && onImportComplete ? (
        <Label kind={kinds.SUCCESS}>{translate('OnImportComplete')}</Label>
      ) : null}

      {supportsOnRename && onRename ? (
        <Label kind={kinds.SUCCESS}>{translate('OnRename')}</Label>
      ) : null}

      {supportsOnHealthIssue && onHealthIssue ? (
        <Label kind={kinds.SUCCESS}>{translate('OnHealthIssue')}</Label>
      ) : null}

      {supportsOnHealthRestored && onHealthRestored ? (
        <Label kind={kinds.SUCCESS}>{translate('OnHealthRestored')}</Label>
      ) : null}

      {supportsOnApplicationUpdate && onApplicationUpdate ? (
        <Label kind={kinds.SUCCESS}>{translate('OnApplicationUpdate')}</Label>
      ) : null}

      {supportsOnSeriesAdd && onSeriesAdd ? (
        <Label kind={kinds.SUCCESS}>{translate('OnSeriesAdd')}</Label>
      ) : null}

      {supportsOnSeriesDelete && onSeriesDelete ? (
        <Label kind={kinds.SUCCESS}>{translate('OnSeriesDelete')}</Label>
      ) : null}

      {supportsOnEpisodeFileDelete && onEpisodeFileDelete ? (
        <Label kind={kinds.SUCCESS}>{translate('OnEpisodeFileDelete')}</Label>
      ) : null}

      {supportsOnEpisodeFileDeleteForUpgrade &&
      onEpisodeFileDelete &&
      onEpisodeFileDeleteForUpgrade ? (
        <Label kind={kinds.SUCCESS}>
          {translate('OnEpisodeFileDeleteForUpgrade')}
        </Label>
      ) : null}

      {supportsOnManualInteractionRequired && onManualInteractionRequired ? (
        <Label kind={kinds.SUCCESS}>
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
        <Label kind={kinds.DISABLED} outline={true}>
          {translate('Disabled')}
        </Label>
      ) : null}

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
