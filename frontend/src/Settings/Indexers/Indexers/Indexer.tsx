import React, { useCallback, useState } from 'react';
import ProtocolLabel from 'Activity/Queue/ProtocolLabel';
import Card from 'Components/Card';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import TagList from 'Components/TagList';
import { icons, kinds } from 'Helpers/Props';
import { ProviderTestStatus } from 'Settings/ProviderTestAllResult';
import { useTagList } from 'Tags/useTags';
import translate from 'Utilities/String/translate';
import { IndexerModel, useDeleteIndexer } from '../useIndexers';
import EditIndexerModal from './EditIndexerModal';
import styles from './Indexer.css';

interface IndexerProps extends IndexerModel {
  testStatus?: ProviderTestStatus;
  showPriority: boolean;
  onCloneIndexerPress: (id: number) => void;
}

function getTestStatusLabel(status: ProviderTestStatus) {
  switch (status) {
    case 'passed':
      return translate('ProviderTestPassed');
    case 'warning':
      return translate('ProviderTestWarningStatus');
    case 'failed':
      return translate('ProviderTestFailedStatus');
    default:
      return translate('ProviderNotTested');
  }
}

function getTestStatusKind(status: ProviderTestStatus) {
  switch (status) {
    case 'passed':
      return kinds.SUCCESS;
    case 'warning':
      return kinds.WARNING;
    case 'failed':
      return kinds.DANGER;
    default:
      return kinds.DISABLED;
  }
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
  const testStatusLabel = testStatus
    ? getTestStatusLabel(testStatus)
    : undefined;

  return (
    <Card
      className={styles.indexer}
      overlayContent={true}
      aria-label={
        testStatusLabel
          ? translate('EditIndexerNameWithTestStatus', {
              name,
              testStatus: testStatusLabel,
            })
          : translate('EditIndexerName', { name })
      }
      onPress={handleEditIndexerPress}
    >
      <div className={styles.nameContainer}>
        <div className={styles.name}>{name}</div>

        <IconButton
          className={styles.cloneButton}
          title={translate('CloneIndexer')}
          aria-label={translate('CloneIndexer')}
          name={icons.CLONE}
          onPress={handleCloneIndexerPress}
        />
      </div>

      <div className={styles.enabled}>
        <ProtocolLabel protocol={protocol} />

        {supportsRss && enableRss ? (
          <Label kind={kinds.SUCCESS}>{translate('Rss')}</Label>
        ) : null}

        {supportsSearch && enableAutomaticSearch ? (
          <Label kind={kinds.SUCCESS}>{translate('AutomaticSearch')}</Label>
        ) : null}

        {supportsSearch && enableInteractiveSearch ? (
          <Label kind={kinds.SUCCESS}>{translate('InteractiveSearch')}</Label>
        ) : null}

        {showPriority ? (
          <Label kind={kinds.DEFAULT}>
            {translate('Priority')}: {priority}
          </Label>
        ) : null}

        {testStatus && testStatusLabel ? (
          <Label kind={getTestStatusKind(testStatus)}>{testStatusLabel}</Label>
        ) : null}

        {!enableRss && !enableAutomaticSearch && !enableInteractiveSearch ? (
          <Label kind={kinds.DISABLED} outline={true}>
            {translate('Disabled')}
          </Label>
        ) : null}
      </div>

      <TagList tags={tags} tagList={tagList} />

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
    </Card>
  );
}

export default Indexer;
