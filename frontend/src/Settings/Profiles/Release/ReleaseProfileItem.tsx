import React, {
  MouseEvent,
  ReactNode,
  useCallback,
  useMemo,
  useState,
} from 'react';
import Label from 'Components/Label';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import SettingsCard from 'Components/SettingsCard/SettingsCard';
import SettingsCardAction from 'Components/SettingsCard/SettingsCardAction';
import SettingsCardStatus from 'Components/SettingsCard/SettingsCardStatus';
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

const CHIP_CAP_THRESHOLD = 6;

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
    tagList,
    indexerList,
  } = props;

  const { deleteReleaseProfile, isDeleting } = useDeleteReleaseProfile(id);

  const [isEditModalOpen, setEditModalOpen, setEditModalClosed] =
    useModalOpenState(false);

  const [isDeleteModalOpen, setDeleteModalOpen, setDeleteModalClosed] =
    useModalOpenState(false);

  const [isExpanded, setIsExpanded] = useState(false);

  const allChips = useMemo(
    () => [...required, ...ignored].filter(Boolean),
    [required, ignored]
  );

  const isCapped = !isExpanded && allChips.length > CHIP_CAP_THRESHOLD;

  const indexers = useMemo(
    () => indexerList.filter((i) => indexerIds.includes(i.id)),
    [indexerList, indexerIds]
  );

  const tagNames = useMemo(
    () => tagList.filter((t) => tags.includes(t.id)),
    [tagList, tags]
  );

  const handleShowAllPress = useCallback((e: MouseEvent) => {
    e.stopPropagation();
    setIsExpanded(true);
  }, []);

  const statusSegments = useMemo(() => {
    const segments: ReactNode[] = [translate(enabled ? 'Enabled' : 'Disabled')];

    if (indexers.length > 0) {
      segments.push(
        `${indexers.length} ${translate(
          indexers.length === 1 ? 'Indexer' : 'Indexers'
        )}`
      );
    }

    if (tagNames.length > 0) {
      segments.push(
        `${tagNames.length} ${translate(
          tagNames.length === 1 ? 'Tag' : 'Tags'
        )}`
      );
    }

    return segments;
  }, [enabled, indexers, tagNames]);

  return (
    <SettingsCard
      name={name || translate('Unnamed')}
      isUnnamed={!name}
      aria-label={translate('EditReleaseProfileName', { name: name ?? id })}
      actions={
        <>
          <SettingsCardAction
            title={translate('EditReleaseProfile')}
            aria-label={translate('EditReleaseProfile')}
            name={icons.EDIT}
            onPress={setEditModalOpen}
          />

          <SettingsCardAction
            title={translate('DeleteReleaseProfile')}
            aria-label={translate('DeleteReleaseProfile')}
            name={icons.DELETE}
            onPress={setDeleteModalOpen}
          />
        </>
      }
      onPress={setEditModalOpen}
    >
      <SettingsCardStatus
        dot={enabled ? 'active' : 'muted'}
        segments={statusSegments}
      />

      <div className={isCapped ? styles.chipsClipped : styles.chips}>
        {required.map((item) => {
          if (!item) return null;

          return (
            <Label key={item} kind={kinds.SUCCESS}>
              {item}
            </Label>
          );
        })}

        {ignored.map((item) => {
          if (!item) return null;

          return (
            <Label key={item} kind={kinds.DANGER}>
              {item}
            </Label>
          );
        })}
      </div>

      {isCapped ? (
        <button
          className={styles.showAll}
          type="button"
          onClick={handleShowAllPress}
        >
          {`${translate('ShowAllPatterns', { count: allChips.length })} ↓`}
        </button>
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
