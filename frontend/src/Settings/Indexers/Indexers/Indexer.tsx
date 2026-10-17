import React, { useCallback, useState } from 'react';
import ProtocolLabel from 'Activity/Queue/ProtocolLabel';
import Label from 'Components/Label';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import SettingsCard from 'Components/SettingsCard/SettingsCard';
import settingsCardStyles from 'Components/SettingsCard/SettingsCard.module.css';
import SettingsCardAction from 'Components/SettingsCard/SettingsCardAction';
import SettingsCardStatus from 'Components/SettingsCard/SettingsCardStatus';
import TagList from 'Components/TagList';
import { icons, kinds, sizes } from 'Helpers/Props';
import {
  getProviderTestStatusLabel,
  ProviderTestStatus,
} from 'Settings/ProviderTestAllResult';
import { useTagList } from 'Tags/useTags';
import translate from 'Utilities/String/translate';
import { IndexerModel, useDeleteIndexer } from '../useIndexers';
import EditIndexerModal from './EditIndexerModal';

interface IndexerProps extends IndexerModel {
  testStatus?: ProviderTestStatus;
  showPriority: boolean;
  onCloneIndexerPress: (id: number) => void;
}

function Indexer({
  id,
  name,
  protocol,
  enableRss,
  enableAutomaticSearch,
  enableInteractiveSearch,
  tags,
  supportsRss,
  supportsSearch,
  priority,
  testStatus,
  showPriority,
  onCloneIndexerPress,
}: IndexerProps) {
  const tagList = useTagList();
  const { deleteIndexer } = useDeleteIndexer(id);

  const [isEditIndexerModalOpen, setIsEditIndexerModalOpen] = useState(false);
  const [isDeleteIndexerModalOpen, setIsDeleteIndexerModalOpen] =
    useState(false);

  const handleEditIndexerPress = useCallback(() => {
    setIsEditIndexerModalOpen(true);
  }, []);

  const handleEditIndexerModalClose = useCallback(() => {
    setIsEditIndexerModalOpen(false);
  }, []);

  const handleDeleteIndexerPress = useCallback(() => {
    setIsEditIndexerModalOpen(false);
    setIsDeleteIndexerModalOpen(true);
  }, []);

  const handleDeleteIndexerModalClose = useCallback(() => {
    setIsDeleteIndexerModalOpen(false);
  }, []);

  const handleConfirmDeleteIndexer = useCallback(() => {
    deleteIndexer();
  }, [deleteIndexer]);

  const handleCloneIndexerPress = useCallback(() => {
    onCloneIndexerPress(id);
  }, [id, onCloneIndexerPress]);

  const enabled = enableRss || enableAutomaticSearch || enableInteractiveSearch;
  const testStatusLabel = testStatus
    ? getProviderTestStatusLabel(testStatus)
    : undefined;

  return (
    <SettingsCard
      name={name}
      aria-label={
        testStatusLabel
          ? translate('EditIndexerNameWithTestStatus', {
              name,
              testStatus: testStatusLabel,
            })
          : translate('EditIndexerName', { name })
      }
      actions={
        <SettingsCardAction
          title={translate('CloneIndexer')}
          aria-label={translate('CloneIndexer')}
          name={icons.CLONE}
          onPress={handleCloneIndexerPress}
        />
      }
      onPress={handleEditIndexerPress}
    >
      <SettingsCardStatus
        dot={enabled ? 'active' : 'muted'}
        segments={[
          translate(enabled ? 'Enabled' : 'Disabled'),
          showPriority ? (
            <>
              {translate('Priority')} {priority}
            </>
          ) : null,
          testStatusLabel,
        ]}
      />

      <div className={settingsCardStyles.labels}>
        <ProtocolLabel protocol={protocol} size={sizes.MEDIUM} />

        {supportsRss && enableRss ? (
          <Label dot={false} kind={kinds.DEFAULT} size={sizes.MEDIUM}>
            {translate('Rss')}
          </Label>
        ) : null}

        {supportsSearch && enableAutomaticSearch ? (
          <Label dot={false} kind={kinds.DEFAULT} size={sizes.MEDIUM}>
            {translate('AutomaticSearch')}
          </Label>
        ) : null}

        {supportsSearch && enableInteractiveSearch ? (
          <Label dot={false} kind={kinds.DEFAULT} size={sizes.MEDIUM}>
            {translate('InteractiveSearch')}
          </Label>
        ) : null}
      </div>

      {tags.length > 0 ? <TagList tags={tags} tagList={tagList} /> : null}

      <EditIndexerModal
        id={id}
        isOpen={isEditIndexerModalOpen}
        onModalClose={handleEditIndexerModalClose}
        onDeleteIndexerPress={handleDeleteIndexerPress}
      />

      <ConfirmModal
        isOpen={isDeleteIndexerModalOpen}
        kind={kinds.DANGER}
        title={translate('DeleteIndexer')}
        message={translate('DeleteIndexerMessageText', { name })}
        confirmLabel={translate('Delete')}
        onConfirm={handleConfirmDeleteIndexer}
        onCancel={handleDeleteIndexerModalClose}
      />
    </SettingsCard>
  );
}

export default Indexer;
