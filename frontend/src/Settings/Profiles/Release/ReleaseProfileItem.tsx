import React, { useCallback, useMemo, useState } from 'react';
import Card from 'Components/Card';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
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

  const handleShowAllPress = useCallback((e: React.MouseEvent) => {
    e.stopPropagation();
    setIsExpanded(true);
  }, []);

  const statusSegments = useMemo(() => {
    const segments: string[] = [];

    if (indexers.length > 0) {
      segments.push(
        `${indexers.length} ${indexers.length === 1 ? 'INDEXER' : 'INDEXERS'}`
      );
    }

    if (tagNames.length > 0) {
      segments.push(
        `${tagNames.length} ${tagNames.length === 1 ? 'TAG' : 'TAGS'}`
      );
    }

    return segments;
  }, [indexers, tagNames]);

  return (
    <Card
      className={styles.releaseProfile}
      overlayContent={true}
      aria-label={translate('EditReleaseProfileName', { name: name ?? id })}
      onPress={setEditModalOpen}
    >
      <div className={styles.nameContainer}>
        <div className={styles.name}>{name || translate('ReleaseProfile')}</div>

        <div className={styles.rightCluster}>
          <IconButton
            className={styles.actionButton}
            title={translate('EditReleaseProfile')}
            aria-label={translate('EditReleaseProfile')}
            name={icons.EDIT}
            onPress={setEditModalOpen}
          />

          <IconButton
            className={styles.actionButton}
            title={translate('DeleteReleaseProfile')}
            aria-label={translate('DeleteReleaseProfile')}
            name={icons.DELETE}
            onPress={setDeleteModalOpen}
          />
        </div>
      </div>

      <div className={styles.statusLine}>
        {enabled ? <span className={styles.statusDot} /> : null}
        <span>{enabled ? 'ENABLED' : 'DISABLED'}</span>
        {statusSegments.map((seg, i) => (
          <React.Fragment key={i}>
            <span className={styles.statusSeparator}>·</span>
            <span>{seg}</span>
          </React.Fragment>
        ))}
      </div>

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
          {`Show all ${allChips.length} patterns ↓`}
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
    </Card>
  );
}

export default ReleaseProfileItem;
