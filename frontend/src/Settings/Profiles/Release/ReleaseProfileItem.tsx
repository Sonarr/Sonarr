import React, { useMemo } from 'react';
import Label from 'Components/Label';
import MiddleTruncate from 'Components/MiddleTruncate';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import SettingsCard from 'Components/SettingsCard/SettingsCard';
import settingsCardStyles from 'Components/SettingsCard/SettingsCard.css';
import SettingsCardAction from 'Components/SettingsCard/SettingsCardAction';
import SettingsCardStatus from 'Components/SettingsCard/SettingsCardStatus';
import TagList from 'Components/TagList';
import useModalOpenState from 'Helpers/Hooks/useModalOpenState';
import { icons, kinds } from 'Helpers/Props';
import { IndexerModel } from 'Settings/Indexers/useIndexers';
import { Tag } from 'Tags/useTags';
import translate from 'Utilities/String/translate';
import EditReleaseProfileModal from './EditReleaseProfileModal';
import {
  ReleaseProfileModel,
  useDeleteReleaseProfile,
} from './useReleaseProfiles';
import styles from './ReleaseProfileItem.css';

interface ReleaseProfileProps extends ReleaseProfileModel {
  tagList: ReadonlyArray<Tag>;
  indexerList: ReadonlyArray<IndexerModel>;
}

function ReleaseProfileItem(props: ReleaseProfileProps) {
  const {
    id,
    name,
    enabled = true,
    required = [],
    ignored = [],
    indexerIds = [],
    tags,
    excludedTags,
    tagList,
    indexerList,
  } = props;

  const { deleteReleaseProfile, isDeleting } = useDeleteReleaseProfile(id);

  const [isEditModalOpen, setEditModalOpen, setEditModalClosed] =
    useModalOpenState(false);

  const [isDeleteModalOpen, setDeleteModalOpen, setDeleteModalClosed] =
    useModalOpenState(false);

  const indexers = useMemo(
    () => indexerList.filter((i) => indexerIds.includes(i.id)),
    [indexerList, indexerIds]
  );

  const statusSegments = [translate(enabled ? 'Enabled' : 'Disabled')];

  return (
    <SettingsCard
      name={name || translate('Unnamed')}
      isUnnamed={!name}
      aria-label={translate('EditReleaseProfileName', { name: name ?? id })}
      actions={
        <SettingsCardAction
          title={translate('DeleteReleaseProfile')}
          aria-label={translate('DeleteReleaseProfile')}
          name={icons.DELETE}
          onPress={setDeleteModalOpen}
        />
      }
      onPress={setEditModalOpen}
    >
      <SettingsCardStatus
        dot={enabled ? 'active' : 'muted'}
        segments={statusSegments}
      />

      <div className={styles.chips}>
        {required.map((item) => {
          if (!item) return null;

          return (
            <Label
              key={item}
              className={settingsCardStyles.truncatedLabel}
              kind={kinds.SUCCESS}
            >
              <MiddleTruncate text={item} />
            </Label>
          );
        })}

        {ignored.map((item) => {
          if (!item) return null;

          return (
            <Label
              key={item}
              className={settingsCardStyles.truncatedLabel}
              kind={kinds.DANGER}
            >
              <MiddleTruncate text={item} />
            </Label>
          );
        })}
      </div>

      {tags.length ? <TagList tags={tags} tagList={tagList} /> : null}

      {excludedTags.length ? (
        <TagList tags={excludedTags} tagList={tagList} kind={kinds.DANGER} />
      ) : null}

      {indexers.length ? (
        <div className={settingsCardStyles.labels}>
          {indexers.map((indexer) => (
            <Label key={indexer.id} kind={kinds.INFO} outline={true}>
              {indexer.name}
            </Label>
          ))}
        </div>
      ) : null}

      <EditReleaseProfileModal
        id={id}
        isOpen={isEditModalOpen}
        onModalClose={setEditModalClosed}
        onDeleteReleaseProfilePress={setDeleteModalOpen}
      />

      <ConfirmModal
        isOpen={isDeleteModalOpen}
        kind={kinds.DANGER}
        title={translate('DeleteReleaseProfile')}
        message={translate('DeleteReleaseProfileMessageText', {
          name: name ?? id,
        })}
        confirmLabel={translate('Delete')}
        isSpinning={isDeleting}
        onConfirm={deleteReleaseProfile}
        onCancel={setDeleteModalClosed}
      />
    </SettingsCard>
  );
}

export default ReleaseProfileItem;
