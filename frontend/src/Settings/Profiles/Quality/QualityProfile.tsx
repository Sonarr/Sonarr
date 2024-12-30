import React, { useCallback, useState } from 'react';
import { useDispatch } from 'react-redux';
import Card from 'Components/Card';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import Tooltip from 'Components/Tooltip/Tooltip';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import { deleteQualityProfile } from 'Store/Actions/settingsActions';
import { QualityProfileItems } from 'typings/QualityProfile';
import translate from 'Utilities/String/translate';
import EditQualityProfileModal from './EditQualityProfileModal';
import styles from './QualityProfile.css';

interface QualityProfileProps {
  id: number;
  name: string;
  upgradeAllowed: boolean;
  cutoff: number;
  items: QualityProfileItems;

  isDeleting: boolean;
  onCloneQualityProfilePress: (id: number) => void;
}

function QualityProfile({
  id,
  name,
  upgradeAllowed,
  cutoff,
  items,
  isDeleting,
  onCloneQualityProfilePress,
}: QualityProfileProps) {
  const dispatch = useDispatch();

  const [isEditQualityProfileModalOpen, setIsEditQualityProfileModalOpen] =
    useState(false);

  const [isDeleteQualityProfileModalOpen, setIsDeleteQualityProfileModalOpen] =
    useState(false);

  const handleEditQualityProfilePress = useCallback(() => {
    setIsEditQualityProfileModalOpen(true);
  }, []);

  const handleEditQualityProfileModalClose = useCallback(() => {
    setIsEditQualityProfileModalOpen(false);
  }, []);

  const handleDeleteQualityProfilePress = useCallback(() => {
    setIsDeleteQualityProfileModalOpen(true);
  }, []);

  const handleDeleteQualityProfileModalClose = useCallback(() => {
    setIsDeleteQualityProfileModalOpen(false);
  }, []);

  const handleConfirmDeleteQualityProfile = useCallback(() => {
    dispatch(deleteQualityProfile({ id }));
  }, [id, dispatch]);

  const handleCloneQualityProfilePress = useCallback(() => {
    onCloneQualityProfilePress(id);
  }, [id, onCloneQualityProfilePress]);

  return (
    <Card
      className={styles.qualityProfile}
      overlayContent={true}
      onPress={handleEditQualityProfilePress}
    >
      <div className={styles.nameContainer}>
        <div className={styles.name}>{name}</div>

        <IconButton
          className={styles.cloneButton}
          title={translate('CloneProfile')}
          name={icons.CLONE}
          onPress={handleCloneQualityProfilePress}
        />
      </div>

      <div className={styles.qualities}>
        {items.map((item) => {
          if (!item.allowed) {
            return null;
          }

          if ('quality' in item) {
            const isCutoff = upgradeAllowed && item.quality.id === cutoff;

            return (
              <Label
                key={item.quality.id}
                kind={isCutoff ? kinds.INFO : kinds.DEFAULT}
                title={
                  isCutoff
                    ? translate('UpgradeUntilThisQualityIsMetOrExceeded')
                    : undefined
                }
              >
                {item.quality.name}
              </Label>
            );
          }

          const isCutoff = upgradeAllowed && item.id === cutoff;

          return (
            <Tooltip
              key={item.id}
              className={styles.tooltipLabel}
              anchor={
                <Label
                  kind={isCutoff ? kinds.INFO : kinds.DEFAULT}
                  title={isCutoff ? translate('Cutoff') : undefined}
                >
                  {item.name}
                </Label>
              }
              tooltip={
                <div>
                  {item.items.map((groupItem) => {
                    return (
                      <Label
                        key={groupItem.quality.id}
                        kind={isCutoff ? kinds.INFO : kinds.DEFAULT}
                        title={isCutoff ? translate('Cutoff') : undefined}
                      >
                        {groupItem.quality.name}
                      </Label>
                    );
                  })}
                </div>
              }
              kind={kinds.INVERSE}
              position={tooltipPositions.TOP}
            />
          );
        })}
      </div>

      <EditQualityProfileModal
        id={id}
        isOpen={isEditQualityProfileModalOpen}
        onModalClose={handleEditQualityProfileModalClose}
        onDeleteQualityProfilePress={handleDeleteQualityProfilePress}
      />

      <ConfirmModal
        isOpen={isDeleteQualityProfileModalOpen}
        kind={kinds.DANGER}
        title={translate('DeleteQualityProfile')}
        message={translate('DeleteQualityProfileMessageText', { name })}
        confirmLabel={translate('Delete')}
        isSpinning={isDeleting}
        onConfirm={handleConfirmDeleteQualityProfile}
        onCancel={handleDeleteQualityProfileModalClose}
      />
    </Card>
  );
}

export default QualityProfile;
