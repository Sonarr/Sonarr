import { CollisionPriority } from '@dnd-kit/abstract';
import { useDragOperation } from '@dnd-kit/react';
import { useSortable } from '@dnd-kit/react/sortable';
import classNames from 'classnames';
import React, { useCallback } from 'react';
import CheckInput from 'Components/Form/CheckInput';
import TextInput from 'Components/Form/TextInput';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import SortableHandle, {
  SortableHandleOption,
} from 'Components/Sortable/SortableHandle';
import { icons } from 'Helpers/Props';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import QualityProfileItem from './QualityProfileItem';
import { ItemFailures } from './qualityProfileItemFailures';
import type { QualityProfileMoveTarget } from './QualityProfileItems';
import { SizeChanged } from './QualityProfileItemSize';
import {
  groupContainerKey,
  ROOT_CONTAINER,
  rootCollisionDetector,
} from './useQualityProfileDnd';
import { QualityProfileQualityItem } from './useQualityProfiles';
import styles from './QualityProfileItemGroup.module.css';

interface QualityProfileItemGroupProps {
  mode?: string;
  index: number;
  groupId: number;
  name: string;
  allowed: boolean;
  items: QualityProfileQualityItem[];
  failuresByQualityId: Map<number, ItemFailures>;
  isFirst: boolean;
  isLast: boolean;
  moveTargets: QualityProfileMoveTarget[];
  onGroupAllowedChange: (groupId: number, allowed: boolean) => void;
  onItemAllowedChange: (groupId: number, allowed: boolean) => void;
  onItemGroupNameChange: (groupId: number, name: string) => void;
  onDeleteGroupPress: (groupId: number) => void;
  onSizeChange: (sizeChange: SizeChanged) => void;
  onMoveItem: (itemId: string, containerId: string, offset: number) => void;
  onMoveItemTo: (
    itemId: string,
    sourceContainerId: string,
    targetContainerId: string
  ) => void;
}

function QualityProfileItemGroup({
  mode = 'default',
  index,
  groupId,
  name,
  allowed,
  items,
  failuresByQualityId,
  isFirst,
  isLast,
  moveTargets,
  onDeleteGroupPress,
  onGroupAllowedChange,
  onItemAllowedChange,
  onItemGroupNameChange,
  onSizeChange,
  onMoveItem,
  onMoveItemTo,
}: QualityProfileItemGroupProps) {
  const itemId = groupContainerKey(groupId);
  const { ref, handleRef, isDragging } = useSortable({
    id: itemId,
    index,
    group: ROOT_CONTAINER,
    type: 'group',
    accept: ['quality', 'group'],
    collisionDetector: rootCollisionDetector,
    collisionPriority:
      mode === 'editGroups' ? CollisionPriority.Low : CollisionPriority.Normal,
    disabled: mode === 'editSizes',
  });

  const { source } = useDragOperation();

  const handleAllowedChange = useCallback(
    ({ value }: InputChanged<boolean>) => {
      onGroupAllowedChange?.(groupId, value);
    },
    [groupId, onGroupAllowedChange]
  );

  const handleNameChange = useCallback(
    ({ value }: InputChanged<string>) => {
      onItemGroupNameChange?.(groupId, value);
    },
    [groupId, onItemGroupNameChange]
  );

  const handleDeleteGroupPress = useCallback(() => {
    onDeleteGroupPress?.(groupId);
  }, [groupId, onDeleteGroupPress]);

  const moveOptions: SortableHandleOption[] = [
    {
      isDisabled: isFirst,
      label: translate('MoveItemUp', { item: name }),
      onPress: () => onMoveItem(itemId, ROOT_CONTAINER, -1),
    },
    {
      isDisabled: isLast,
      label: translate('MoveItemDown', { item: name }),
      onPress: () => onMoveItem(itemId, ROOT_CONTAINER, 1),
    },
  ];

  return (
    <div
      ref={ref}
      className={classNames(
        styles.qualityProfileItemGroup,
        mode === 'editSizes' && styles.editSizes,
        isDragging && styles.isDragging
      )}
    >
      <div className={styles.qualityProfileItemGroupInfo}>
        {mode === 'editGroups' ? (
          <div className={styles.qualityNameContainer}>
            <IconButton
              className={styles.deleteGroupButton}
              name={icons.UNGROUP}
              title={translate('Ungroup')}
              aria-label={translate('Ungroup')}
              onPress={handleDeleteGroupPress}
            />

            <TextInput
              className={styles.nameInput}
              name="name"
              value={name}
              onChange={handleNameChange}
            />
          </div>
        ) : null}

        {mode === 'default' ? (
          <label className={styles.qualityNameLabel}>
            <CheckInput
              className={styles.checkInput}
              containerClassName={styles.checkInputContainer}
              name="allowed"
              value={allowed}
              onChange={handleAllowedChange}
            />

            <div className={styles.nameContainer}>
              <div
                className={classNames(
                  styles.name,
                  !allowed && styles.notAllowed
                )}
              >
                {name}
              </div>

              <div className={styles.groupQualities}>
                {items.map(({ quality }) => {
                  return (
                    <Label key={quality.id} outline={true}>
                      {quality.name}
                    </Label>
                  );
                })}
              </div>
            </div>
          </label>
        ) : null}

        {mode === 'editSizes' ? (
          <label className={styles.editSizesQualityNameLabel}>
            <div className={styles.nameContainer}>
              <div
                className={classNames(
                  styles.name,
                  !allowed && styles.notAllowed
                )}
              >
                {name}
              </div>
            </div>
          </label>
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

      {mode === 'default' ? null : (
        <div
          className={classNames(
            mode === 'editGroups' && styles.items,
            mode === 'editGroups' &&
              source?.type === 'quality' &&
              styles.isDragActive
          )}
        >
          {items.map((subItem, subIndex) => {
            const { quality, minSize, maxSize, preferredSize } = subItem;

            return (
              <QualityProfileItem
                key={quality.id}
                mode={mode}
                containerId={groupContainerKey(groupId)}
                index={subIndex}
                groupId={groupId}
                qualityId={quality.id}
                name={quality.name}
                allowed={allowed}
                minSize={minSize}
                maxSize={maxSize}
                preferredSize={preferredSize}
                failures={failuresByQualityId.get(quality.id)}
                isFirst={subIndex === 0}
                isLast={subIndex === items.length - 1}
                moveTargets={moveTargets}
                onItemAllowedChange={onItemAllowedChange}
                onSizeChange={onSizeChange}
                onMoveItem={onMoveItem}
                onMoveItemTo={onMoveItemTo}
              />
            );
          })}
        </div>
      )}
    </div>
  );
}

export default QualityProfileItemGroup;
