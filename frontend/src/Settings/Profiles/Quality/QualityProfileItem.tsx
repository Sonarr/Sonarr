import { useSortable } from '@dnd-kit/react/sortable';
import classNames from 'classnames';
import React, { useCallback } from 'react';
import CheckInput from 'Components/Form/CheckInput';
import IconButton from 'Components/Link/IconButton';
import SortableHandle, {
  SortableHandleOption,
} from 'Components/Sortable/SortableHandle';
import { icons } from 'Helpers/Props';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import { ItemFailures } from './qualityProfileItemFailures';
import type { QualityProfileMoveTarget } from './QualityProfileItems';
import QualityProfileItemSize, { SizeChanged } from './QualityProfileItemSize';
import {
  qualityKey,
  ROOT_CONTAINER,
  rootCollisionDetector,
} from './useQualityProfileDnd';
import styles from './QualityProfileItem.module.css';

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
  isFirst: boolean;
  isLast: boolean;
  moveTargets: QualityProfileMoveTarget[];
  onCreateGroupPress?: (qualityId: number) => void;
  onItemAllowedChange: (qualityId: number, allowed: boolean) => void;
  onSizeChange: (change: SizeChanged) => void;
  onMoveItem: (itemId: string, containerId: string, offset: number) => void;
  onMoveItemTo: (
    itemId: string,
    sourceContainerId: string,
    targetContainerId: string
  ) => void;
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
  isFirst,
  isLast,
  moveTargets,
  onCreateGroupPress,
  onItemAllowedChange,
  onSizeChange,
  onMoveItem,
  onMoveItemTo,
}: QualityProfileItemProps) {
  const itemId = qualityKey(qualityId);
  const { ref, handleRef, isDragging } = useSortable({
    id: itemId,
    index,
    group: containerId,
    type: 'quality',
    accept: containerId === ROOT_CONTAINER ? undefined : ['quality'],
    collisionDetector:
      containerId === ROOT_CONTAINER ? rootCollisionDetector : undefined,
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

  const moveOptions: SortableHandleOption[] = [
    {
      isDisabled: isFirst,
      label: translate('MoveItemUp', { item: name }),
      onPress: () => onMoveItem(itemId, containerId, -1),
    },
    {
      isDisabled: isLast,
      label: translate('MoveItemDown', { item: name }),
      onPress: () => onMoveItem(itemId, containerId, 1),
    },
    ...(containerId === ROOT_CONTAINER
      ? []
      : [
          {
            label: translate('MoveItemOutsideGroup', { item: name }),
            onPress: () => onMoveItemTo(itemId, containerId, ROOT_CONTAINER),
          },
        ]),
    ...moveTargets
      .filter((target) => target.id !== containerId)
      .map((target) => ({
        label: translate('MoveItemTo', {
          item: name,
          destination: target.name,
        }),
        onPress: () => onMoveItemTo(itemId, containerId, target.id),
      })),
  ];

  return (
    <div
      ref={ref}
      className={classNames(
        styles.qualityProfileItem,
        mode === 'editSizes' && styles.editSizes,
        isDragging && styles.isDragging
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
        <SortableHandle
          className={styles.dragHandle}
          handleRef={handleRef}
          label={name}
          options={moveOptions}
        />
      )}
    </div>
  );
}

export default QualityProfileItem;
