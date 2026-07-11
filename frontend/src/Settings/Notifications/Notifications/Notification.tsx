import React, { useCallback, useState } from 'react';
import Label from 'Components/Label';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import SettingsCard from 'Components/SettingsCard/SettingsCard';
import settingsCardStyles from 'Components/SettingsCard/SettingsCard.css';
import SettingsCardStatus from 'Components/SettingsCard/SettingsCardStatus';
import TagList from 'Components/TagList';
import { kinds, sizes } from 'Helpers/Props';
import { useTagList } from 'Tags/useTags';
import translate from 'Utilities/String/translate';
import { NotificationModel, useDeleteConnection } from '../useConnections';
import EditNotificationModal from './EditNotificationModal';

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

  const enabled =
    onGrab ||
    onDownload ||
    onRename ||
    onImportComplete ||
    onHealthIssue ||
    onHealthRestored ||
    onApplicationUpdate ||
    onSeriesAdd ||
    onSeriesDelete ||
    onEpisodeFileDelete ||
    onManualInteractionRequired;

  return (
    <SettingsCard
      name={name}
      aria-label={translate('EditConnectionName', { name })}
      onPress={handleEditNotificationPress}
    >
      <SettingsCardStatus
        dot={enabled ? 'active' : 'muted'}
        segments={[translate(enabled ? 'Enabled' : 'Disabled')]}
      />

      <div className={settingsCardStyles.labels}>
        {supportsOnGrab && onGrab ? (
          <Label dot={false} kind={kinds.DEFAULT} size={sizes.MEDIUM}>
            {translate('OnGrab')}
          </Label>
        ) : null}

        {supportsOnDownload && onDownload ? (
          <Label dot={false} kind={kinds.DEFAULT} size={sizes.MEDIUM}>
            {translate('OnFileImport')}
          </Label>
        ) : null}

        {supportsOnUpgrade && onDownload && onUpgrade ? (
          <Label dot={false} kind={kinds.DEFAULT} size={sizes.MEDIUM}>
            {translate('OnFileUpgrade')}
          </Label>
        ) : null}

        {supportsOnImportComplete && onImportComplete ? (
          <Label dot={false} kind={kinds.DEFAULT} size={sizes.MEDIUM}>
            {translate('OnImportComplete')}
          </Label>
        ) : null}

        {supportsOnRename && onRename ? (
          <Label dot={false} kind={kinds.DEFAULT} size={sizes.MEDIUM}>
            {translate('OnRename')}
          </Label>
        ) : null}

        {supportsOnHealthIssue && onHealthIssue ? (
          <Label dot={false} kind={kinds.DEFAULT} size={sizes.MEDIUM}>
            {translate('OnHealthIssue')}
          </Label>
        ) : null}

        {supportsOnHealthRestored && onHealthRestored ? (
          <Label dot={false} kind={kinds.DEFAULT} size={sizes.MEDIUM}>
            {translate('OnHealthRestored')}
          </Label>
        ) : null}

        {supportsOnApplicationUpdate && onApplicationUpdate ? (
          <Label dot={false} kind={kinds.DEFAULT} size={sizes.MEDIUM}>
            {translate('OnApplicationUpdate')}
          </Label>
        ) : null}

        {supportsOnSeriesAdd && onSeriesAdd ? (
          <Label dot={false} kind={kinds.DEFAULT} size={sizes.MEDIUM}>
            {translate('OnSeriesAdd')}
          </Label>
        ) : null}

        {supportsOnSeriesDelete && onSeriesDelete ? (
          <Label dot={false} kind={kinds.DEFAULT} size={sizes.MEDIUM}>
            {translate('OnSeriesDelete')}
          </Label>
        ) : null}

        {supportsOnEpisodeFileDelete && onEpisodeFileDelete ? (
          <Label dot={false} kind={kinds.DEFAULT} size={sizes.MEDIUM}>
            {translate('OnEpisodeFileDelete')}
          </Label>
        ) : null}

        {supportsOnEpisodeFileDeleteForUpgrade &&
        onEpisodeFileDelete &&
        onEpisodeFileDeleteForUpgrade ? (
          <Label dot={false} kind={kinds.DEFAULT} size={sizes.MEDIUM}>
            {translate('OnEpisodeFileDeleteForUpgrade')}
          </Label>
        ) : null}

        {supportsOnManualInteractionRequired && onManualInteractionRequired ? (
          <Label dot={false} kind={kinds.DEFAULT} size={sizes.MEDIUM}>
            {translate('OnManualInteractionRequired')}
          </Label>
        ) : null}
      </div>

      {tags.length > 0 ? <TagList tags={tags} tagList={tagList} /> : null}

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
    </SettingsCard>
  );
}

export default Notification;
