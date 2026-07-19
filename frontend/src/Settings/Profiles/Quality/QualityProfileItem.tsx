import { useSortable } from '@dnd-kit/react/sortable';
import classNames from 'classnames';
import React, { useCallback } from 'react';
import CheckInput from 'Components/Form/CheckInput';
import Icon from 'Components/Icon';
import IconButton from 'Components/Link/IconButton';
import { icons } from 'Helpers/Props';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import { ItemFailures } from './qualityProfileItemFailures';
import QualityProfileItemSize, { SizeChanged } from './QualityProfileItemSize';
import { qualityKey, ROOT_CONTAINER } from './useQualityProfileDnd';
import styles from './QualityProfileItem.css';

interface QualityProfileItemProps {
  mode: string;
  containerId: string;
  index: number;
  groupId?: number;
  qualityId: number;
  name: string;
  allowed: boolean;
  minSize: number | null;
  maxSize: number | null;
  preferredSize: number | null;
  failures?: ItemFailures;
  onCreateGroupPress?: (qualityId: number) => void;
  onItemAllowedChange: (qualityId: number, allowed: boolean) => void;
  onSizeChange: (change: SizeChanged) => void;
}

function QualityProfileItem({
  mode = 'default',
  containerId,
  index,
  qualityId,
  groupId,
  name,
  allowed,
  minSize,
  maxSize,
  preferredSize,
  failures,
  onCreateGroupPress,
  onItemAllowedChange,
  onSizeChange,
}: QualityProfileItemProps) {
  const { ref, handleRef, isDragging } = useSortable({
    id: qualityKey(qualityId),
    index,
    group: containerId,
    type: 'quality',
    accept: containerId === ROOT_CONTAINER ? undefined : ['quality'],
    disabled: mode === 'editSizes',
  });

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
      ref={ref}
      className={classNames(
        styles.qualityProfileItem,
        mode === 'editSizes' && styles.editSizes,
        isDragging && styles.isDragging,
        groupId && styles.isInGroup
      )}
    >
      <label
        className={classNames(
          styles.qualityNameContainer,
          mode === 'editSizes' && styles.editSizes
        )}
      >
        {mode === 'editGroups' && !groupId ? (
          <IconButton
            className={styles.createGroupButton}
            name={icons.GROUP}
            title={translate('Group')}
            aria-label={translate('Group')}
            onPress={handleCreateGroupPress}
          />
        ) : null}

        {mode === 'default' ? (
          <CheckInput
            className={styles.checkInput}
            containerClassName={styles.checkInputContainer}
            name={name}
            value={allowed}
            isDisabled={!!groupId}
            onChange={handleAllowedChange}
          />
        ) : null}

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
            failures={failures}
            onSizeChange={onSizeChange}
          />
        </div>
      ) : null}

      {mode === 'editSizes' ? null : (
        <div ref={handleRef} className={styles.dragHandle}>
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
