import React, { useCallback, useState } from 'react';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import SettingsCard from 'Components/SettingsCard/SettingsCard';
import SettingsCardAction from 'Components/SettingsCard/SettingsCardAction';
import SettingsCardStatus from 'Components/SettingsCard/SettingsCardStatus';
import TagList from 'Components/TagList';
import { icons, kinds } from 'Helpers/Props';
import { useTagList } from 'Tags/useTags';
import formatShortTimeSpan from 'Utilities/Date/formatShortTimeSpan';
import translate from 'Utilities/String/translate';
import EditImportListModal from './EditImportListModal';
import { useDeleteImportList } from './useImportLists';

interface ImportListProps {
  id: number;
  name: string;
  enableAutomaticAdd: boolean;
  minRefreshInterval: string;
  tags: number[];
  tagExisting: boolean;
  onCloneImportListPress: (id: number) => void;
}

function ImportList({
  id,
  name,
  enableAutomaticAdd,
  minRefreshInterval,
  tags,
  onCloneImportListPress,
}: ImportListProps) {
  const tagList = useTagList();
  const { deleteImportList } = useDeleteImportList(id);

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
    deleteImportList();
  }, [deleteImportList]);

  const handleCloneImportListPress = useCallback(() => {
    onCloneImportListPress(id);
  }, [id, onCloneImportListPress]);

  return (
    <SettingsCard
      name={name}
      aria-label={translate('EditImportListName', { name })}
      actions={
        <SettingsCardAction
          title={translate('CloneImportList')}
          aria-label={translate('CloneImportList')}
          name={icons.CLONE}
          onPress={handleCloneImportListPress}
        />
      }
      onPress={handleEditImportListPress}
    >
      <SettingsCardStatus
        dot={enableAutomaticAdd ? 'active' : 'muted'}
        segments={[
          enableAutomaticAdd ? translate('AutomaticAdd') : null,
          <>
            {translate('Refresh')}: {formatShortTimeSpan(minRefreshInterval)}
          </>,
        ]}
      />

      {tags.length > 0 ? <TagList tags={tags} tagList={tagList} /> : null}

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
    </SettingsCard>
  );
}

export default ImportList;
