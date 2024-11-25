import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import { Tag } from 'App/State/TagsAppState';
import Card from 'Components/Card';
import Label from 'Components/Label';
import MiddleTruncate from 'Components/MiddleTruncate';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import TagList from 'Components/TagList';
import useModalOpenState from 'Helpers/Hooks/useModalOpenState';
import { kinds } from 'Helpers/Props';
import { deleteReleaseProfile } from 'Store/Actions/Settings/releaseProfiles';
import Indexer from 'typings/Indexer';
import ReleaseProfile from 'typings/Settings/ReleaseProfile';
import translate from 'Utilities/String/translate';
import EditReleaseProfileModal from './EditReleaseProfileModal';
import styles from './ReleaseProfileItem.css';

interface ReleaseProfileProps extends ReleaseProfile {
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
    indexerId = 0,
    tagList,
    indexerList,
  } = props;

  const dispatch = useDispatch();

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
    dispatch(deleteReleaseProfile({ id }));
  }, [id, dispatch]);

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
