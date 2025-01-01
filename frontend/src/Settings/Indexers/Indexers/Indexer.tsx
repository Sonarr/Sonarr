import React, { useCallback, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import Card from 'Components/Card';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import TagList from 'Components/TagList';
import { icons, kinds } from 'Helpers/Props';
import { deleteIndexer } from 'Store/Actions/settingsActions';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import IndexerModel from 'typings/Indexer';
import translate from 'Utilities/String/translate';
import EditIndexerModal from './EditIndexerModal';
import styles from './Indexer.css';

interface IndexerProps extends IndexerModel {
  showPriority: boolean;
  onCloneIndexerPress: (id: number) => void;
}

function Indexer({
  id,
  name,
  enableRss,
  enableAutomaticSearch,
  enableInteractiveSearch,
  tags,
  supportsRss,
  supportsSearch,
  priority,
  showPriority,
  onCloneIndexerPress,
}: IndexerProps) {
  const dispatch = useDispatch();
  const tagList = useSelector(createTagsSelector());

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
    dispatch(deleteIndexer({ id }));
  }, [id, dispatch]);

  const handleCloneIndexerPress = useCallback(() => {
    onCloneIndexerPress(id);
  }, [id, onCloneIndexerPress]);

  return (
    <Card
      className={styles.indexer}
      overlayContent={true}
      onPress={handleEditIndexerPress}
    >
      <div className={styles.nameContainer}>
        <div className={styles.name}>{name}</div>

        <IconButton
          className={styles.cloneButton}
          title={translate('CloneIndexer')}
          name={icons.CLONE}
          onPress={handleCloneIndexerPress}
        />
      </div>

      <div className={styles.enabled}>
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
