import classNames from 'classnames';
import React, { useRef } from 'react';
import { DragSourceMonitor, useDrag, useDrop, XYCoord } from 'react-dnd';
import DragType from 'Helpers/DragType';
import QualityProfileItem from './QualityProfileItem';
import { ItemFailures, ItemFailuresMap } from './qualityProfileItemFailures';
import QualityProfileItemGroup from './QualityProfileItemGroup';
import { SizeChanged } from './QualityProfileItemSize';
import { QualityProfileQualityItem } from './useQualityProfiles';
import styles from './QualityProfileItemDragSource.css';

export interface DragMoveState {
  dragQualityIndex: string | null;
  dropQualityIndex: string | null;
  dropPosition: 'above' | 'below' | null;
}

interface DragItem {
  mode: string;
  qualityIndex: string;
  groupId: number | undefined;
  qualityId: number | undefined;
  isGroup: boolean;
  name: string;
  allowed: boolean;
}

interface ItemProps {
  groupId: number | undefined;
  qualityId: number;
  name: string;
  minSize: number | null;
  maxSize: number | null;
  preferredSize: number | null;
  failures?: ItemFailures;
  isInGroup?: boolean;
  onCreateGroupPress?: (qualityId: number) => void;
  onItemAllowedChange: (id: number, allowed: boolean) => void;
}

interface GroupProps {
  groupId: number;
  qualityId: undefined;
  items: QualityProfileQualityItem[];
  qualityIndex: string;
  itemFailures?: ItemFailuresMap;
  onDeleteGroupPress: (groupId: number) => void;
  onItemAllowedChange: (id: number, allowed: boolean) => void;
  onGroupAllowedChange: (id: number, allowed: boolean) => void;
  onItemGroupNameChange: (groupId: number, name: string) => void;
}

interface CommonProps {
  mode: string;
  name: string;
  allowed: boolean;
  qualityIndex: string;
  isDraggingUp: boolean;
  isDraggingDown: boolean;
  onDragMove: (move: DragMoveState) => void;
  onDragEnd: (didDrop: boolean) => void;
  onSizeChange: (sizeChange: SizeChanged) => void;
}

export type QualityProfileItemDragSourceProps = CommonProps &
  (ItemProps | GroupProps);

export interface QualityProfileItemDragSourceActionProps {
  onCreateGroupPress?: (qualityId: number) => void;
  onItemAllowedChange: (id: number, allowed: boolean) => void;
  onDeleteGroupPress: (groupId: number) => void;
  onGroupAllowedChange: (id: number, allowed: boolean) => void;
  onItemGroupNameChange: (groupId: number, name: string) => void;
  onDragMove: (move: DragMoveState) => void;
  onDragEnd: (didDrop: boolean) => void;
  onSizeChange: (sizeChange: SizeChanged) => void;
}

function QualityProfileItemDragSource({
  mode,
  groupId,
  qualityId,
  name,
  allowed,
  qualityIndex,
  isDraggingDown,
  isDraggingUp,
  onDragMove,
  onDragEnd,
  ...otherProps
}: QualityProfileItemDragSourceProps) {
  const ref = useRef<HTMLDivElement>(null);
  const itemRef = useRef<HTMLDivElement | null>(null);
  const lastMoveRef = useRef<string | null>(null);

  const [{ isOver }, dropRef] = useDrop<DragItem, void, { isOver: boolean }>({
    accept: DragType.QualityProfileItem,
    collect(monitor) {
      return {
        isOver: monitor.isOver({ shallow: true }),
      };
    },
    hover(item: DragItem, monitor) {
      if (!itemRef.current || !monitor.isOver({ shallow: true })) {
        lastMoveRef.current = null;
        return;
      }

      const { qualityIndex: dragQualityIndex, isGroup: isDragGroup } = item;
      const dropQualityIndex = qualityIndex;
      const isDropGroupItem = !!(qualityId && groupId);

      if (dragQualityIndex === dropQualityIndex) {
        return;
      }

      if (isDragGroup && isDropGroupItem) {
        return;
      }

      const rect = itemRef.current.getBoundingClientRect();
      const deadZone = rect.height * 0.15;
      const offsetY = (monitor.getClientOffset() as XYCoord).y - rect.top;

      let dropPosition: 'above' | 'below' | null = null;

      if (offsetY > rect.height / 2 + deadZone) {
        dropPosition = 'below';
      } else if (offsetY < rect.height / 2 - deadZone) {
        dropPosition = 'above';
      } else {
        return;
      }

      const moveKey = `${dropQualityIndex}:${dropPosition}`;

      if (lastMoveRef.current === moveKey) {
        return;
      }

      lastMoveRef.current = moveKey;

      onDragMove({ dragQualityIndex, dropQualityIndex, dropPosition });
    },
  });

  const [{ isDragging }, dragRef, previewRef] = useDrag<
    DragItem,
    unknown,
    { isDragging: boolean }
  >({
    type: DragType.QualityProfileItem,
    item: () => {
      return {
        mode,
        qualityIndex,
        groupId,
        qualityId,
        isGroup: !qualityId,
        name,
        allowed,
      };
    },
    collect: (monitor: DragSourceMonitor<unknown, unknown>) => ({
      isDragging: monitor.isDragging(),
    }),
    end: (_item: DragItem, monitor) => {
      lastMoveRef.current = null;
      onDragEnd(monitor.didDrop());
    },
  });

  dropRef(previewRef(ref));

  const isBefore = !isDragging && isDraggingUp && isOver;
  const isAfter = !isDragging && isDraggingDown && isOver;

  return (
    <div ref={ref} className={classNames(styles.qualityProfileItemDragSource)}>
      {isBefore ? (
        <div className={styles.qualityProfileItemPlaceholder} />
      ) : null}

      <div ref={itemRef}>
        {'items' in otherProps && groupId ? (
          <QualityProfileItemGroup
            {...otherProps}
            dragRef={dragRef}
            mode={mode}
            groupId={groupId}
            name={name}
            allowed={allowed}
            qualityIndex={qualityIndex}
            isDragging={isDragging}
            isDraggingUp={isDraggingUp}
            isDraggingDown={isDraggingDown}
            onDragEnd={onDragEnd}
            onDragMove={onDragMove}
          />
        ) : null}

        {!('items' in otherProps) && qualityId ? (
          <QualityProfileItem
            {...otherProps}
            dragRef={dragRef}
            mode={mode}
            groupId={groupId}
            qualityId={qualityId}
            name={name}
            allowed={allowed}
            isDragging={isDragging}
          />
        ) : null}
      </div>

      {isAfter ? (
        <div className={styles.qualityProfileItemPlaceholder} />
      ) : null}
    </div>
  );
}

export default QualityProfileItemDragSource;
