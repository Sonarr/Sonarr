import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import Card from 'Components/Card';
import Label from 'Components/Label';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import useModalOpenState from 'Helpers/Hooks/useModalOpenState';
import { kinds } from 'Helpers/Props';
import { deleteNotificationTemplate } from 'Store/Actions/Settings/notificationTemplates';
import NotificationTemplate from 'typings/Settings/NotificationTemplate';
import translate from 'Utilities/String/translate';
import EditNotificationTemplateModal from './EditNotificationTemplateModal';
import styles from './NotificationTemplateItem.css';

interface NotificationTemplateProps extends NotificationTemplate {
  title: string;
  body: string;
  onGrab: boolean;
  onDownload: boolean;
  onUpgrade: boolean;
  onImportComplete: boolean;
  onRename: boolean;
  onSeriesAdd: boolean;
  onSeriesDelete: boolean;
  onEpisodeFileDelete: boolean;
  onEpisodeFileDeleteForUpgrade: boolean;
  onHealthIssue: boolean;
  onHealthRestored: boolean;
  onApplicationUpdate: boolean;
  onManualInteractionRequired: boolean;
}

function NotificationTemplateItem(props: NotificationTemplateProps) {
  const {
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
    onManualInteractionRequired
  } = props;

  const dispatch = useDispatch();

  const [
    isEditNotificationTemplateModalOpen,
    setEditNotificationTemplateModalOpen,
    setEditNotificationTemplateModalClosed,
  ] = useModalOpenState(false);

  const [
    isDeleteNotificationTemplateModalOpen,
    setDeleteNotificationTemplateModalOpen,
    setDeleteNotificationTemplateModalClosed,
  ] = useModalOpenState(false);

  const handleDeletePress = useCallback(() => {
    dispatch(deleteNotificationTemplate({ id }));
  }, [id, dispatch]);

  return (
    <Card
      className={styles.notificationTemplate}
      overlayContent={true}
      onPress={setEditNotificationTemplateModalOpen}
    >
      {name ? <div className={styles.name}>{name}</div> : null}
      {
        onGrab ?
          <Label kind={kinds.SUCCESS}>
            {translate('OnGrab')}
          </Label> :
          null
      }
      {
        onDownload ?
          <Label kind={kinds.SUCCESS}>
            {translate('OnFileImport')}
          </Label> :
          null
      }
      {
        onUpgrade ?
          <Label kind={kinds.SUCCESS}>
            {translate('OnFileUpgrade')}
          </Label> :
          null
      }
      {
        onImportComplete ?
          <Label kind={kinds.SUCCESS}>
            {translate('OnImportComplete')}
          </Label> :
          null
      }
      {
        onRename ?
          <Label kind={kinds.SUCCESS}>
            {translate('OnRename')}
          </Label> :
          null
      }
      {
        onSeriesAdd ?
          <Label kind={kinds.SUCCESS}>
            {translate('OnSeriesAdd')}
          </Label> :
          null
      }
      {
        onSeriesDelete ?
          <Label kind={kinds.SUCCESS}>
            {translate('OnSeriesDelete')}
          </Label> :
          null
      }
      {
        onEpisodeFileDelete ?
          <Label kind={kinds.SUCCESS}>
            {translate('OnEpisodeFileDelete')}
          </Label> :
          null
      }
      {
        onEpisodeFileDeleteForUpgrade ?
          <Label kind={kinds.SUCCESS}>
            {translate('OnEpisodeFileDeleteForUpgrade')}
          </Label> :
          null
      }
      {
        onHealthIssue ?
          <Label kind={kinds.SUCCESS}>
            {translate('OnHealthIssue')}
          </Label> :
          null
      }
      {
        onHealthRestored ?
          <Label kind={kinds.SUCCESS}>
            {translate('OnHealthRestored')}
          </Label> :
          null
      }
      {
        onApplicationUpdate ?
          <Label kind={kinds.SUCCESS}>
            {translate('OnApplicationUpdate')}
          </Label> :
          null
      }
      {
        onManualInteractionRequired ?
          <Label kind={kinds.SUCCESS}>
            {translate('OnManualInteractionRequired')}
          </Label> :
          null
      }

      <EditNotificationTemplateModal
        id={id}
        isOpen={isEditNotificationTemplateModalOpen}
        onModalClose={setEditNotificationTemplateModalClosed}
        onDeleteNotificationTemplatePress={setDeleteNotificationTemplateModalOpen}
      />

      <ConfirmModal
        isOpen={isDeleteNotificationTemplateModalOpen}
        kind={kinds.DANGER}
        title={translate('DeleteNotificationTemplate')}
        message={translate('DeleteNotificationTemplateMessageText', {
          name: name ?? id,
        })}
        confirmLabel={translate('Delete')}
        onConfirm={handleDeletePress}
        onCancel={setDeleteNotificationTemplateModalClosed}
      />
    </Card>
  );
}

export default NotificationTemplateItem;
