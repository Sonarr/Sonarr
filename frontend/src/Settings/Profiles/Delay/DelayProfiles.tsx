import classNames from 'classnames';
import React, { useCallback, useState } from 'react';
import Link from 'Components/Link/Link';
import PageSectionContent from 'Components/Page/PageSectionContent';
import { useTagList } from 'Tags/useTags';
import translate from 'Utilities/String/translate';
import DelayProfile from './DelayProfile';
import EditDelayProfileModal from './EditDelayProfileModal';
import {
  useReorderDelayProfile,
  useSortedDelayProfiles,
} from './useDelayProfiles';
import styles from './DelayProfiles.css';

function DelayProfiles() {
  const {
    error,
    isFetching,
    isFetched: isPopulated,
    items,
    defaultProfile,
  } = useSortedDelayProfiles();

  const { reorderDelayProfile } = useReorderDelayProfile();

  const tagList = useTagList();

  const [dragIndex, setDragIndex] = useState<number | null>(null);
  const [dropIndex, setDropIndex] = useState<number | null>(null);
  const [isAddDelayProfileModalOpen, setIsAddDelayProfileModalOpen] =
    useState(false);

  const isDragging = dropIndex !== null;
  const isDraggingUp =
    isDragging &&
    dropIndex != null &&
    dragIndex != null &&
    dropIndex < dragIndex;
  const isDraggingDown =
    isDragging &&
    dropIndex != null &&
    dragIndex != null &&
    dropIndex > dragIndex;

  const handleAddDelayProfilePress = useCallback(() => {
    setIsAddDelayProfileModalOpen(true);
  }, []);

  const handleAddDelayProfileModalClose = useCallback(() => {
    setIsAddDelayProfileModalOpen(false);
  }, []);

  const handleDelayProfileDragMove = useCallback(
    (newDragIndex: number, newDropIndex: number) => {
      setDragIndex(newDragIndex);
      setDropIndex(newDropIndex);
    },
    []
  );

  const handleDelayProfileDragEnd = useCallback(
    (id: number, didDrop: boolean) => {
      if (didDrop && dropIndex !== null) {
        const moveOrder = dropIndex;
        const moving = items.find((p) => p.id === id);

        if (moving && moving.order !== moveOrder) {
          const after =
            moveOrder > 1 ? items.find((p) => p.order === moveOrder - 1) : null;

          reorderDelayProfile({ id, after: after?.id });
        }
      }

      setDragIndex(null);
      setDropIndex(null);
    },
    [dropIndex, items, reorderDelayProfile]
  );

  return (
    <PageSectionContent
      errorMessage={translate('DelayProfilesLoadError')}
      error={error}
      isFetching={isFetching}
      isPopulated={isPopulated}
    >
      <div className={styles.delayList}>
        <div className={styles.headerRow}>
          <div className={styles.colDrag} />
          <div className={classNames(styles.colScope, styles.headerCell)}>
            {translate('Tags')}
          </div>
          <div className={classNames(styles.colProto, styles.headerCell)}>
            {translate('PreferredProtocol')}
          </div>
          <div className={classNames(styles.colUsenet, styles.headerCell)}>
            {translate('UsenetDelay')}
          </div>
          <div className={classNames(styles.colTorrent, styles.headerCell)}>
            {translate('TorrentDelay')}
          </div>
          <div className={styles.colActions} />
        </div>

        {items.map((item) => {
          return (
            <DelayProfile
              key={item.id}
              {...item}
              tagList={tagList}
              isDraggingUp={isDraggingUp}
              isDraggingDown={isDraggingDown}
              onDelayProfileDragEnd={handleDelayProfileDragEnd}
              onDelayProfileDragMove={handleDelayProfileDragMove}
            />
          );
        })}

        {defaultProfile ? (
          <DelayProfile
            {...defaultProfile}
            tagList={tagList}
            isDraggingDown={false}
            isDraggingUp={false}
            onDelayProfileDragEnd={handleDelayProfileDragEnd}
            onDelayProfileDragMove={handleDelayProfileDragMove}
          />
        ) : null}

        <Link className={styles.ghostRow} onPress={handleAddDelayProfilePress}>
          {translate('AddDelayProfile')}
        </Link>
      </div>

      <EditDelayProfileModal
        isOpen={isAddDelayProfileModalOpen}
        onModalClose={handleAddDelayProfileModalClose}
      />
    </PageSectionContent>
  );
}

export default DelayProfiles;
