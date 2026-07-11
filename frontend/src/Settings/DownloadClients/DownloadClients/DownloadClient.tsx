import React, { useCallback, useState } from 'react';
import ProtocolLabel from 'Activity/Queue/ProtocolLabel';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import SettingsCard from 'Components/SettingsCard/SettingsCard';
import settingsCardStyles from 'Components/SettingsCard/SettingsCard.css';
import SettingsCardAction from 'Components/SettingsCard/SettingsCardAction';
import SettingsCardStatus from 'Components/SettingsCard/SettingsCardStatus';
import TagList from 'Components/TagList';
import DownloadProtocol from 'DownloadClient/DownloadProtocol';
import { icons, kinds, sizes } from 'Helpers/Props';
import { useTagList } from 'Tags/useTags';
import translate from 'Utilities/String/translate';
import EditDownloadClientModal from './EditDownloadClientModal';
import { useDeleteDownloadClient } from './useDownloadClients';

interface DownloadClientProps {
  id: number;
  name: string;
  protocol: DownloadProtocol;
  enable: boolean;
  priority: number;
  tags: number[];
  onCloneDownloadClientPress: (id: number) => void;
}

function DownloadClient({
  id,
  name,
  protocol,
  enable,
  priority,
  tags,
  onCloneDownloadClientPress,
}: DownloadClientProps) {
  const tagList = useTagList();
  const { deleteDownloadClient } = useDeleteDownloadClient(id);

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
    deleteDownloadClient();
  }, [deleteDownloadClient]);

  const handleCloneDownloadClientPress = useCallback(() => {
    onCloneDownloadClientPress(id);
  }, [id, onCloneDownloadClientPress]);

  return (
    <SettingsCard
      name={name}
      aria-label={translate('EditDownloadClientName', { name })}
      actions={
        <SettingsCardAction
          title={translate('CloneDownloadClient')}
          aria-label={translate('CloneDownloadClient')}
          name={icons.CLONE}
          onPress={handleCloneDownloadClientPress}
        />
      }
      onPress={handleEditDownloadClientPress}
    >
      <SettingsCardStatus
        dot={enable ? 'active' : 'muted'}
        segments={[
          translate(enable ? 'Enabled' : 'Disabled'),
          priority > 1 ? translate('PrioritySettings', { priority }) : null,
        ]}
      />

      <div className={settingsCardStyles.labels}>
        <ProtocolLabel protocol={protocol} size={sizes.MEDIUM} />
      </div>

      {tags.length > 0 ? <TagList tags={tags} tagList={tagList} /> : null}

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
    </SettingsCard>
  );
}

export default DownloadClient;
