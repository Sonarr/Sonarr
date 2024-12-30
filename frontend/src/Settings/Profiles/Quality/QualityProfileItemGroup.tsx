import classNames from 'classnames';
import React, { useCallback } from 'react';
import { ConnectDragSource } from 'react-dnd';
import CheckInput from 'Components/Form/CheckInput';
import TextInput from 'Components/Form/TextInput';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import { icons } from 'Helpers/Props';
import { InputChanged } from 'typings/inputs';
import { QualityProfileQualityItem } from 'typings/QualityProfile';
import translate from 'Utilities/String/translate';
import QualityProfileItemDragSource, {
  DragMoveState,
} from './QualityProfileItemDragSource';
import { SizeChanged } from './QualityProfileItemSize';
import styles from './QualityProfileItemGroup.css';

interface QualityProfileItemGroupProps {
  dragRef: ConnectDragSource;
  mode?: string;
  groupId: number;
  name: string;
  allowed: boolean;
  items: QualityProfileQualityItem[];
  qualityIndex: string;
  isDragging: boolean;
  isDraggingUp: boolean;
  isDraggingDown: boolean;
  onGroupAllowedChange: (groupId: number, allowed: boolean) => void;
  onItemAllowedChange: (groupId: number, allowed: boolean) => void;
  onItemGroupNameChange: (groupId: number, name: string) => void;
  onDeleteGroupPress: (groupId: number) => void;
  onDragMove: (drag: DragMoveState) => void;
  onDragEnd: (didDrop: boolean) => void;
  onSizeChange: (sizeChange: SizeChanged) => void;
}

function QualityProfileItemGroup({
  dragRef,
  mode = 'default',
  groupId,
  name,
  allowed,
  items,
  qualityIndex,
  isDragging,
  isDraggingUp,
  isDraggingDown,
  onDeleteGroupPress,
  onGroupAllowedChange,
  onItemAllowedChange,
  onItemGroupNameChange,
  onDragMove,
  onDragEnd,
  onSizeChange,
}: QualityProfileItemGroupProps) {
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
      className={classNames(
        styles.qualityProfileItemGroup,
        mode === 'editGroups' && styles.editGroups,
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
                {items
                  .map(({ quality }) => {
                    return <Label key={quality.id}>{quality.name}</Label>;
                  })
                  .reverse()}
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
          <div ref={dragRef} className={styles.dragHandle}>
            <Icon
              className={styles.dragIcon}
              name={icons.REORDER}
              title={translate('Reorder')}
            />
          </div>
        )}
      </div>

      {mode === 'default' ? null : (
        <div className={mode === 'editGroups' ? styles.items : undefined}>
          {items
            .map(({ quality }, index) => {
              return (
                <QualityProfileItemDragSource
                  key={quality.id}
                  mode={mode}
                  groupId={groupId}
                  qualityId={quality.id}
                  name={quality.name}
                  allowed={allowed}
                  minSize={quality.minSize}
                  maxSize={quality.maxSize}
                  preferredSize={quality.preferredSize}
                  qualityIndex={`${qualityIndex}.${index + 1}`}
                  isDraggingUp={isDraggingUp}
                  isDraggingDown={isDraggingDown}
                  isInGroup={true}
                  onItemAllowedChange={onItemAllowedChange}
                  onDragMove={onDragMove}
                  onDragEnd={onDragEnd}
                  onSizeChange={onSizeChange}
                />
              );
            })
            .reverse()}
        </div>
      )}
    </div>
  );
}

export default QualityProfileItemGroup;
