import { CollisionPriority } from '@dnd-kit/abstract';
import { useDragOperation } from '@dnd-kit/react';
import { useSortable } from '@dnd-kit/react/sortable';
import classNames from 'classnames';
import React, { useCallback } from 'react';
import CheckInput from 'Components/Form/CheckInput';
import TextInput from 'Components/Form/TextInput';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import { icons } from 'Helpers/Props';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import QualityProfileItem from './QualityProfileItem';
import { ItemFailures } from './qualityProfileItemFailures';
import { SizeChanged } from './QualityProfileItemSize';
import { groupContainerKey, ROOT_CONTAINER } from './useQualityProfileDnd';
import { QualityProfileQualityItem } from './useQualityProfiles';
import styles from './QualityProfileItemGroup.css';

interface QualityProfileItemGroupProps {
  mode?: string;
  index: number;
  groupId: number;
  name: string;
  allowed: boolean;
  items: QualityProfileQualityItem[];
  failuresByQualityId: Map<number, ItemFailures>;
  onGroupAllowedChange: (groupId: number, allowed: boolean) => void;
  onItemAllowedChange: (groupId: number, allowed: boolean) => void;
  onItemGroupNameChange: (groupId: number, name: string) => void;
  onDeleteGroupPress: (groupId: number) => void;
  onSizeChange: (sizeChange: SizeChanged) => void;
}

function QualityProfileItemGroup({
  mode = 'default',
  index,
  groupId,
  name,
  allowed,
  items,
  failuresByQualityId,
  onDeleteGroupPress,
  onGroupAllowedChange,
  onItemAllowedChange,
  onItemGroupNameChange,
  onSizeChange,
}: QualityProfileItemGroupProps) {
  const { ref, handleRef, isDragging } = useSortable({
    id: groupContainerKey(groupId),
    index,
    group: ROOT_CONTAINER,
    type: 'group',
    accept: ['quality', 'group'],
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
          <div ref={handleRef} className={styles.dragHandle}>
            <Icon
              className={styles.dragIcon}
              name={icons.REORDER}
              title={translate('Reorder')}
            />
          </div>
        )}
      </div>

      {mode === 'default' ? null : (
        <div
          className={classNames(
            mode === 'editGroups' && styles.items,
            mode === 'editGroups' && source && styles.isDragActive
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
                onItemAllowedChange={onItemAllowedChange}
                onSizeChange={onSizeChange}
              />
            );
          })}
        </div>
      )}
    </div>
  );
}

export default QualityProfileItemGroup;
