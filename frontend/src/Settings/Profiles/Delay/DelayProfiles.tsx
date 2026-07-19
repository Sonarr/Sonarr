import { move } from '@dnd-kit/helpers';
import { DragDropProvider, DragEndEvent, DragOverEvent } from '@dnd-kit/react';
import React, { useCallback, useState } from 'react';
import Link from 'Components/Link/Link';
import PageSectionContent from 'Components/Page/PageSectionContent';
import { useTagList } from 'Tags/useTags';
import translate from 'Utilities/String/translate';
import DelayProfile from './DelayProfile';
import EditDelayProfileModal from './EditDelayProfileModal';
import {
  DelayProfile as DelayProfileType,
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

  const [localItems, setLocalItems] = useState<DelayProfileType[] | null>(null);
  const [isAddDelayProfileModalOpen, setIsAddDelayProfileModalOpen] =
    useState(false);

  const displayedItems = localItems ?? items;

  const handleAddDelayProfilePress = useCallback(() => {
    setIsAddDelayProfileModalOpen(true);
  }, []);

  const handleAddDelayProfileModalClose = useCallback(() => {
    setIsAddDelayProfileModalOpen(false);
  }, []);

  const handleDragStart = useCallback(() => {
    setLocalItems([...items]);
  }, [items]);

  const handleDragOver = useCallback((event: DragOverEvent) => {
    setLocalItems((current) => (current ? move(current, event) : current));
  }, []);

  const handleDragEnd = useCallback(
    (event: DragEndEvent) => {
      setLocalItems((current) => {
        if (current && !event.canceled) {
          const moved = move(current, event);
          const id = event.operation.source?.id;
          const index = moved.findIndex((item) => item.id === id);

          if (id != null && index !== -1) {
            const after = index > 0 ? moved[index - 1].id : undefined;

            reorderDelayProfile({ id: id as number, after });
          }
        }

        return null;
      });
    },
    [reorderDelayProfile]
  );

  return (
    <PageSectionContent
      errorMessage={translate('DelayProfilesLoadError')}
      error={error}
      isFetching={isFetching}
      isPopulated={isPopulated}
    >
      <div className={styles.headerRow}>
        <div className={styles.colDrag} />

        <div className={styles.colScope}>{translate('Tags')}</div>

        <div className={styles.colProto}>{translate('PreferredProtocol')}</div>

        <div className={styles.colUsenet}>{translate('UsenetDelay')}</div>

        <div className={styles.colTorrent}>{translate('TorrentDelay')}</div>

        <div className={styles.colActions} />
      </div>

      <DragDropProvider
        onDragStart={handleDragStart}
        onDragOver={handleDragOver}
        onDragEnd={handleDragEnd}
      >
        <div className={styles.delayList}>
          {displayedItems.map((item, index) => {
            return (
              <DelayProfile
                key={item.id}
                {...item}
                index={index}
                tagList={tagList}
              />
            );
          })}

          {defaultProfile ? (
            <DelayProfile {...defaultProfile} index={-1} tagList={tagList} />
          ) : null}

          <Link className={styles.addRow} onPress={handleAddDelayProfilePress}>
            {translate('AddDelayProfile')}
          </Link>
        </div>
      </DragDropProvider>

      <EditDelayProfileModal
        isOpen={isAddDelayProfileModalOpen}
        onModalClose={handleAddDelayProfileModalClose}
      />
    </PageSectionContent>
  );
}

export default DelayProfiles;
