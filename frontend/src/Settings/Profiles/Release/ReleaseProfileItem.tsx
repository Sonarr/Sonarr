import React, { useCallback } from 'react';
import Card from 'Components/Card';
import Label from 'Components/Label';
import MiddleTruncate from 'Components/MiddleTruncate';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import TagList from 'Components/TagList';
import useModalOpenState from 'Helpers/Hooks/useModalOpenState';
import { kinds } from 'Helpers/Props';
import { Tag } from 'Tags/useTags';
import Indexer from 'typings/Indexer';
import translate from 'Utilities/String/translate';
import EditReleaseProfileModal from './EditReleaseProfileModal';
import {
  ReleaseProfileModel,
  useDeleteReleaseProfile,
} from './useReleaseProfiles';
import styles from './ReleaseProfileItem.css';

interface ReleaseProfileProps extends ReleaseProfileModel {
  tagList: Tag[];
  indexerList: Indexer[];
}

function ReleaseProfileItem(props: ReleaseProfileProps) {
  const {
    id,
    name,
    enabled = true,
    required = [],
    ignored = [],
    tags,
    excludedTags,
    indexerId = 0,
    tagList,
    indexerList,
  } = props;

  const { deleteReleaseProfile } = useDeleteReleaseProfile(id);

  const [
    isEditReleaseProfileModalOpen,
    setEditReleaseProfileModalOpen,
    setEditReleaseProfileModalClosed,
  ] = useModalOpenState(false);

  const [
    isDeleteReleaseProfileModalOpen,
    setDeleteReleaseProfileModalOpen,
    setDeleteReleaseProfileModalClosed,
  ] = useModalOpenState(false);

  const handleDeletePress = useCallback(() => {
    deleteReleaseProfile();
  }, [deleteReleaseProfile]);

  const indexer =
    indexerId !== 0 && indexerList.find((i) => i.id === indexerId);

  return (
    <Card
      className={styles.releaseProfile}
      overlayContent={true}
      onPress={setEditReleaseProfileModalOpen}
    >
      {name ? <div className={styles.name}>{name}</div> : null}

      <div>
        {required.map((item) => {
          if (!item) {
            return null;
          }

          return (
            <Label key={item} className={styles.label} kind={kinds.SUCCESS}>
              <MiddleTruncate text={item} />
            </Label>
          );
        })}
      </div>

      <div>
        {ignored.map((item) => {
          if (!item) {
            return null;
          }

          return (
            <Label key={item} className={styles.label} kind={kinds.DANGER}>
              <MiddleTruncate text={item} />
            </Label>
          );
        })}
      </div>

      <TagList tags={tags} tagList={tagList} />

      <TagList tags={excludedTags} tagList={tagList} kind={kinds.DANGER} />

      <div>
        {enabled ? null : (
          <Label kind={kinds.DISABLED} outline={true}>
            {translate('Disabled')}
          </Label>
        )}

        {indexer ? (
          <Label kind={kinds.INFO} outline={true}>
            {indexer.name}
          </Label>
        ) : null}
      </div>

      <EditReleaseProfileModal
        id={id}
        isOpen={isEditReleaseProfileModalOpen}
        onModalClose={setEditReleaseProfileModalClosed}
        onDeleteReleaseProfilePress={setDeleteReleaseProfileModalOpen}
      />

      <ConfirmModal
        isOpen={isDeleteReleaseProfileModalOpen}
        kind={kinds.DANGER}
        title={translate('DeleteReleaseProfile')}
        message={translate('DeleteReleaseProfileMessageText', {
          name: name ?? id,
        })}
        confirmLabel={translate('Delete')}
        onConfirm={handleDeletePress}
        onCancel={setDeleteReleaseProfileModalClosed}
      />
    </Card>
  );
}

export default ReleaseProfileItem;
