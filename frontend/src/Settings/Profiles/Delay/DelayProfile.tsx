import { useSortable } from '@dnd-kit/react/sortable';
import classNames from 'classnames';
import React, { useCallback, useMemo, useState } from 'react';
import Card from 'Components/Card';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import TagList from 'Components/TagList';
import { icons, kinds, sizes } from 'Helpers/Props';
import { Tag } from 'Tags/useTags';
import titleCase from 'Utilities/String/titleCase';
import translate from 'Utilities/String/translate';
import EditDelayProfileModal from './EditDelayProfileModal';
import { useDeleteDelayProfile } from './useDelayProfiles';
import styles from './DelayProfile.css';

function getDelay(enabled: boolean, delay: number) {
  if (!enabled) {
    return '-';
  }

  if (!delay) {
    return translate('NoDelay');
  }

  if (delay === 1) {
    return translate('OneMinute');
  }

  // TODO: use better units of time than just minutes
  return translate('DelayMinutes', { delay });
}

interface DelayProfileProps {
  id: number;
  index: number;
  enableUsenet: boolean;
  enableTorrent: boolean;
  preferredProtocol: string;
  usenetDelay: number;
  torrentDelay: number;
  tags: number[];
  tagList: ReadonlyArray<Tag>;
}

function DelayProfile({
  id,
  index,
  enableUsenet,
  enableTorrent,
  preferredProtocol,
  usenetDelay,
  torrentDelay,
  tags,
  tagList,
}: DelayProfileProps) {
  const { deleteDelayProfile } = useDeleteDelayProfile(id);

  const { ref, handleRef, isDragging } = useSortable({
    id,
    index,
    disabled: id === 1,
  });

  const [isEditDelayProfileModalOpen, setIsEditDelayProfileModalOpen] =
    useState(false);

  const [isDeleteDelayProfileModalOpen, setIsDeleteDelayProfileModalOpen] =
    useState(false);

  const preferred = useMemo(() => {
    if (!enableUsenet) {
      return translate('OnlyTorrent');
    } else if (!enableTorrent) {
      return translate('OnlyUsenet');
    }

    return titleCase(translate('PreferProtocol', { preferredProtocol }));
  }, [preferredProtocol, enableUsenet, enableTorrent]);

  const handleEditDelayProfilePress = useCallback(() => {
    setIsEditDelayProfileModalOpen(true);
  }, []);

  const handleEditDelayProfileModalClose = useCallback(() => {
    setIsEditDelayProfileModalOpen(false);
  }, []);

  const handleDeleteDelayProfilePress = useCallback(() => {
    setIsEditDelayProfileModalOpen(false);
    setIsDeleteDelayProfileModalOpen(true);
  }, []);

  const handleDeleteDelayProfileModalClose = useCallback(() => {
    setIsDeleteDelayProfileModalOpen(false);
  }, []);

  const handleConfirmDeleteDelayProfile = useCallback(() => {
    deleteDelayProfile();
  }, [deleteDelayProfile]);

  return (
    <div ref={ref}>
      <Card
        className={classNames(
          styles.delayProfile,
          id === 1 && styles.isDefault,
          isDragging && styles.isDragging
        )}
        overlayClassName={styles.overlay}
        overlayContent={true}
        aria-label={translate('EditDelayProfile')}
        onPress={handleEditDelayProfilePress}
      >
        <div className={styles.colDrag}>
          {id === 1 ? null : (
            <div ref={handleRef} className={styles.dragHandle}>
              <Icon name={icons.REORDER} />
            </div>
          )}
        </div>

        <div className={styles.colScope}>
          {id === 1 ? (
            <Label dot={false} kind={kinds.DEFAULT} size={sizes.MEDIUM}>
              {translate('Default')}
            </Label>
          ) : null}

          {tags.length ? (
            <TagList tags={tags} tagList={tagList} />
          ) : (
            <span className={styles.anyText}>{translate('Any')}</span>
          )}
        </div>

        <div className={styles.colProto}>{preferred}</div>

        <div className={styles.colUsenet}>
          <span className={styles.colLabel}>{translate('UsenetDelay')}</span>
          {getDelay(enableUsenet, usenetDelay)}
        </div>

        <div className={styles.colTorrent}>
          <span className={styles.colLabel}>{translate('TorrentDelay')}</span>
          {getDelay(enableTorrent, torrentDelay)}
        </div>

        <div className={styles.colActions}>
          {id === 1 ? null : (
            <IconButton
              className={styles.actionButton}
              title={translate('DeleteDelayProfile')}
              aria-label={translate('DeleteDelayProfile')}
              name={icons.DELETE}
              onPress={handleDeleteDelayProfilePress}
            />
          )}
        </div>
      </Card>

      <EditDelayProfileModal
        id={id}
        isOpen={isEditDelayProfileModalOpen}
        onModalClose={handleEditDelayProfileModalClose}
        onDeleteDelayProfilePress={handleDeleteDelayProfilePress}
      />

      <ConfirmModal
        isOpen={isDeleteDelayProfileModalOpen}
        kind={kinds.DANGER}
        title={translate('DeleteDelayProfile')}
        message={translate('DeleteDelayProfileMessageText')}
        confirmLabel={translate('Delete')}
        onConfirm={handleConfirmDeleteDelayProfile}
        onCancel={handleDeleteDelayProfileModalClose}
      />
    </div>
  );
}

export default DelayProfile;
