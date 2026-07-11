import React, { useCallback, useMemo, useState } from 'react';
import Card from 'Components/Card';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import Tooltip from 'Components/Tooltip/Tooltip';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import EditQualityProfileModal from './EditQualityProfileModal';
import {
  QualityProfileItems,
  useDeleteQualityProfile,
} from './useQualityProfiles';
import styles from './QualityProfile.css';

const CHIP_CAP_THRESHOLD = 6;

interface QualityProfileProps {
  id: number;
  name: string;
  upgradeAllowed: boolean;
  cutoff: number;
  items: QualityProfileItems;
  onCloneQualityProfilePress: (id: number) => void;
}

function QualityProfile({
  id,
  name,
  upgradeAllowed,
  cutoff,
  items,
  onCloneQualityProfilePress,
}: QualityProfileProps) {
  const { deleteQualityProfile, isDeleting } = useDeleteQualityProfile(id);

  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [isExpanded, setIsExpanded] = useState(false);

  const allowedItems = useMemo(
    () => items.filter((item) => item.allowed),
    [items]
  );

  const cutoffName = useMemo(() => {
    for (const item of items) {
      if (item.allowed) {
        if ('quality' in item) {
          if (item.quality.id === cutoff) return item.quality.name;
        } else if (item.id === cutoff) return item.name;
      }
    }

    return '';
  }, [items, cutoff]);

  const isCapped = !isExpanded && allowedItems.length > CHIP_CAP_THRESHOLD;
  const visibleItems = isCapped
    ? allowedItems.slice(0, CHIP_CAP_THRESHOLD)
    : allowedItems;
  const hiddenCount = allowedItems.length - visibleItems.length;

  const handleEditPress = useCallback(() => {
    setIsEditModalOpen(true);
  }, []);

  const handleEditModalClose = useCallback(() => {
    setIsEditModalOpen(false);
  }, []);

  const handleDeletePress = useCallback(() => {
    setIsEditModalOpen(false);
    setIsDeleteModalOpen(true);
  }, []);

  const handleDeleteModalClose = useCallback(() => {
    setIsDeleteModalOpen(false);
  }, []);

  const handleConfirmDelete = useCallback(() => {
    deleteQualityProfile();
  }, [deleteQualityProfile]);

  const handleClonePress = useCallback(() => {
    onCloneQualityProfilePress(id);
  }, [id, onCloneQualityProfilePress]);

  const handleShowAllPress = useCallback((e: React.MouseEvent) => {
    e.stopPropagation();
    setIsExpanded(true);
  }, []);

  return (
    <Card
      className={styles.qualityProfile}
      overlayClassName={styles.overlay}
      overlayContent={true}
      aria-label={translate('EditQualityProfileName', { name })}
      onPress={handleEditPress}
    >
      <div className={styles.nameContainer}>
        <div className={styles.name}>{name}</div>

        <div className={styles.rightCluster}>
          <IconButton
            className={styles.actionButton}
            title={translate('EditQualityProfile')}
            aria-label={translate('EditQualityProfile')}
            name={icons.EDIT}
            onPress={handleEditPress}
          />

          <IconButton
            className={styles.actionButton}
            title={translate('CloneProfile')}
            aria-label={translate('CloneProfile')}
            name={icons.CLONE}
            onPress={handleClonePress}
          />
        </div>
      </div>

      <div className={styles.statusLine}>
        {upgradeAllowed ? <span className={styles.statusDot} /> : null}
        <span>{upgradeAllowed ? 'UPGRADES ON' : 'UPGRADES OFF'}</span>
        {cutoffName ? (
          <>
            <span className={styles.statusSeparator}>·</span>
            <span>
              CUTOFF <span className={styles.cutoffName}>{cutoffName}</span>
            </span>
          </>
        ) : null}
      </div>

      <div className={styles.chips}>
        {visibleItems.map((item) => {
          if ('quality' in item) {
            const isCutoff = item.quality.id === cutoff;

            return (
              <Label
                key={item.quality.id}
                className={isCutoff ? styles.cutoffChip : undefined}
                outline={!isCutoff}
                title={
                  isCutoff && upgradeAllowed
                    ? translate('UpgradeUntilThisQualityIsMetOrExceeded')
                    : undefined
                }
              >
                {item.quality.name}
              </Label>
            );
          }

          const isCutoff = item.id === cutoff;

          return (
            <Tooltip
              key={item.id}
              anchor={
                <Label
                  className={isCutoff ? styles.cutoffChip : undefined}
                  outline={!isCutoff}
                >
                  {item.name}
                  <span className={styles.groupCount}>
                    +{item.items.length}
                  </span>
                </Label>
              }
              tooltip={
                <div>
                  {item.items.map((groupItem) => (
                    <Label key={groupItem.quality.id} kind={kinds.DEFAULT}>
                      {groupItem.quality.name}
                    </Label>
                  ))}
                </div>
              }
              kind={kinds.INVERSE}
              position={tooltipPositions.TOP}
            />
          );
        })}

        {hiddenCount > 0 ? (
          <button
            className={styles.moreChip}
            type="button"
            onClick={handleShowAllPress}
          >
            {`+${hiddenCount} more`}
          </button>
        ) : null}
      </div>

      <EditQualityProfileModal
        id={id}
        isOpen={isEditModalOpen}
        onModalClose={handleEditModalClose}
        onDeleteQualityProfilePress={handleDeletePress}
      />

      <ConfirmModal
        isOpen={isDeleteModalOpen}
        kind={kinds.DANGER}
        title={translate('DeleteQualityProfile')}
        message={translate('DeleteQualityProfileMessageText', { name })}
        confirmLabel={translate('Delete')}
        isSpinning={isDeleting}
        onConfirm={handleConfirmDelete}
        onCancel={handleDeleteModalClose}
      />
    </Card>
  );
}

export default QualityProfile;
