import classNames from 'classnames';
import React, { useCallback } from 'react';
import { ConnectDragSource } from 'react-dnd';
import CheckInput from 'Components/Form/CheckInput';
import Icon from 'Components/Icon';
import IconButton from 'Components/Link/IconButton';
import { icons } from 'Helpers/Props';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import QualityProfileItemSize, { SizeChanged } from './QualityProfileItemSize';
import styles from './QualityProfileItem.css';

interface QualityProfileItemProps {
  dragRef: ConnectDragSource;
  mode: string;
  isPreview?: boolean;
  groupId?: number;
  qualityId: number;
  name: string;
  allowed: boolean;
  minSize: number | null;
  maxSize: number | null;
  preferredSize: number | null;
  isDragging: boolean;
  onCreateGroupPress?: (qualityId: number) => void;
  onItemAllowedChange: (qualityId: number, allowed: boolean) => void;
  onSizeChange: (change: SizeChanged) => void;
}

function QualityProfileItem({
  dragRef,
  mode = 'default',
  isPreview = false,
  qualityId,
  groupId,
  name,
  allowed,
  minSize,
  maxSize,
  isDragging,
  preferredSize,
  onCreateGroupPress,
  onItemAllowedChange,
  onSizeChange,
}: QualityProfileItemProps) {
  const handleAllowedChange = useCallback(
    ({ value }: InputChanged<boolean>) => {
      onItemAllowedChange?.(qualityId, value);
    },
    [qualityId, onItemAllowedChange]
  );

  const handleCreateGroupPress = useCallback(() => {
    onCreateGroupPress?.(qualityId);
  }, [qualityId, onCreateGroupPress]);

  return (
    <div
      className={classNames(
        styles.qualityProfileItem,
        mode === 'editSizes' && styles.editSizes,
        isDragging && styles.isDragging,
        isPreview && styles.isPreview,
        groupId && styles.isInGroup
      )}
    >
      <label
        className={classNames(
          styles.qualityNameContainer,
          mode === 'editSizes' && styles.editSizes
        )}
      >
        {mode === 'editGroups' && !groupId && !isPreview && (
          <IconButton
            className={styles.createGroupButton}
            name={icons.GROUP}
            title={translate('Group')}
            onPress={handleCreateGroupPress}
          />
        )}

        {mode === 'default' && (
          <CheckInput
            className={styles.checkInput}
            containerClassName={styles.checkInputContainer}
            name={name}
            value={allowed}
            isDisabled={!!groupId}
            onChange={handleAllowedChange}
          />
        )}

        <div
          className={classNames(
            styles.qualityName,
            groupId && mode !== 'editSizes' && styles.isInGroup,
            !allowed && styles.notAllowed
          )}
        >
          {name}
        </div>
      </label>

      {mode === 'editSizes' && qualityId != null ? (
        <div>
          <QualityProfileItemSize
            id={qualityId}
            minSize={minSize}
            maxSize={maxSize}
            preferredSize={preferredSize}
            onSizeChange={onSizeChange}
          />
        </div>
      ) : null}

      {mode === 'editSizes' ? null : (
        <div ref={dragRef} className={styles.dragHandle}>
          <Icon
            className={styles.dragIcon}
            title={translate('CreateGroup')}
            name={icons.REORDER}
          />
        </div>
      )}
    </div>
  );
}

export default QualityProfileItem;
