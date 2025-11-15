import React, { useCallback, useState } from 'react';
import Card from 'Components/Card';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import { kinds } from 'Helpers/Props';
import { useTagDetail } from 'Tags/useTagDetails';
import { useDeleteTag } from 'Tags/useTags';
import translate from 'Utilities/String/translate';
import TagDetailsModal from './Details/TagDetailsModal';
import TagInUse from './TagInUse';
import styles from './Tag.css';

interface TagProps {
  id: number;
  label: string;
}

function Tag({ id, label }: TagProps) {
  const { deleteTag } = useDeleteTag(id);
  const {
    delayProfileIds,
    importListIds,
    notificationIds,
    restrictionIds,
    excludedReleaseProfileIds,
    indexerIds,
    downloadClientIds,
    autoTagIds,
    seriesIds,
  } = useTagDetail(id);
  const [isDetailsModalOpen, setIsDetailsModalOpen] = useState(false);
  const [isDeleteTagModalOpen, setIsDeleteTagModalOpen] = useState(false);

  const isTagUsed = !!(
    delayProfileIds.length ||
    importListIds.length ||
    notificationIds.length ||
    restrictionIds.length ||
    excludedReleaseProfileIds.length ||
    indexerIds.length ||
    downloadClientIds.length ||
    autoTagIds.length ||
    seriesIds.length
  );

  const mergedReleaseProfileIds = Array.from(
    new Set([...restrictionIds, ...excludedReleaseProfileIds]).values()
  );

  const handleShowDetailsPress = useCallback(() => {
    setIsDetailsModalOpen(true);
  }, []);

  const handeDetailsModalClose = useCallback(() => {
    setIsDetailsModalOpen(false);
  }, []);

  const handleDeleteTagPress = useCallback(() => {
    setIsDetailsModalOpen(false);
    setIsDeleteTagModalOpen(true);
  }, []);

  const handleConfirmDeleteTag = useCallback(() => {
    deleteTag();
  }, [deleteTag]);

  const handleDeleteTagModalClose = useCallback(() => {
    setIsDeleteTagModalOpen(false);
  }, []);

  return (
    <Card
      className={styles.tag}
      overlayContent={true}
      onPress={handleShowDetailsPress}
    >
      <div className={styles.label}>{label}</div>

      {isTagUsed ? (
        <div>
          <TagInUse label={translate('Series')} count={seriesIds.length} />

          <TagInUse
            label={translate('DelayProfile')}
            labelPlural={translate('DelayProfiles')}
            count={delayProfileIds.length}
          />

          <TagInUse
            label={translate('ImportList')}
            labelPlural={translate('ImportLists')}
            count={importListIds.length}
          />

          <TagInUse
            label={translate('Connection')}
            labelPlural={translate('Connections')}
            count={notificationIds.length}
          />

          <TagInUse
            label={translate('ReleaseProfile')}
            labelPlural={translate('ReleaseProfiles')}
            count={mergedReleaseProfileIds.length}
          />

          <TagInUse
            label={translate('Indexer')}
            labelPlural={translate('Indexers')}
            count={indexerIds.length}
          />

          <TagInUse
            label={translate('DownloadClient')}
            labelPlural={translate('DownloadClients')}
            count={downloadClientIds.length}
          />

          <TagInUse
            label={translate('AutoTagging')}
            count={autoTagIds.length}
          />
        </div>
      ) : null}

      {!isTagUsed && <div>{translate('NoLinks')}</div>}

      <TagDetailsModal
        label={label}
        isTagUsed={isTagUsed}
        seriesIds={seriesIds}
        delayProfileIds={delayProfileIds}
        importListIds={importListIds}
        notificationIds={notificationIds}
        releaseProfileIds={mergedReleaseProfileIds}
        indexerIds={indexerIds}
        downloadClientIds={downloadClientIds}
        autoTagIds={autoTagIds}
        isOpen={isDetailsModalOpen}
        onModalClose={handeDetailsModalClose}
        onDeleteTagPress={handleDeleteTagPress}
      />

      <ConfirmModal
        isOpen={isDeleteTagModalOpen}
        kind={kinds.DANGER}
        title={translate('DeleteTag')}
        message={translate('DeleteTagMessageText', { label })}
        confirmLabel={translate('Delete')}
        onConfirm={handleConfirmDeleteTag}
        onCancel={handleDeleteTagModalClose}
      />
    </Card>
  );
}

export default Tag;
