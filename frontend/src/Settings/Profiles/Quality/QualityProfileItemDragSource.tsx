import classNames from 'classnames';
import React, { useRef } from 'react';
import { DragSourceMonitor, useDrag, useDrop, XYCoord } from 'react-dnd';
import DragType from 'Helpers/DragType';
import useMeasure from 'Helpers/Hooks/useMeasure';
import { qualityProfileItemHeight } from 'Styles/Variables/dimensions';
import { QualityProfileQualityItem } from 'typings/QualityProfile';
import QualityProfileItem from './QualityProfileItem';
import QualityProfileItemGroup from './QualityProfileItemGroup';
import { SizeChanged } from './QualityProfileItemSize';
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
  height: number;
}

interface ItemProps {
  groupId: number | undefined;
  qualityId: number;
  name: string;
  minSize: number | null;
  maxSize: number | null;
  preferredSize: number | null;
  isInGroup?: boolean;
  onCreateGroupPress?: (qualityId: number) => void;
  onItemAllowedChange: (id: number, allowd: boolean) => void;
}

interface GroupProps {
  groupId: number;
  qualityId: undefined;
  items: QualityProfileQualityItem[];
  qualityIndex: string;
  onDeleteGroupPress: (groupId: number) => void;
  onItemAllowedChange: (id: number, allowd: boolean) => void;
  onGroupAllowedChange: (id: number, allowd: boolean) => void;
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
  onItemAllowedChange: (id: number, allowd: boolean) => void;
  onDeleteGroupPress: (groupId: number) => void;
  onGroupAllowedChange: (id: number, allowd: boolean) => void;
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
  const [measureRef, { height }] = useMeasure();

  const [{ isOver, dragHeight }, dropRef] = useDrop<
    DragItem,
    void,
    { isOver: boolean; dragHeight: number }
  >({
    accept: DragType.QualityProfileItem,
    collect(monitor) {
      return {
        isOver: monitor.isOver(),
        dragHeight: monitor.getItem()?.height ?? qualityProfileItemHeight,
      };
    },
    hover(item: DragItem, monitor) {
      if (!ref.current) {
        return;
      }

      const { qualityIndex: dragQualityIndex, isGroup: isDragGroup } = item;

      const dropQualityIndex = qualityIndex;
      const isDropGroupItem = !!(qualityId && groupId);

      const hoverBoundingRect = ref.current?.getBoundingClientRect();
      const hoverHeight = hoverBoundingRect.bottom - hoverBoundingRect.top;

      // Smooth out updates when dragging down and the size grows to avoid flickering
      const hoverMiddleY = Math.max(hoverHeight - height, height) / 2;

      const clientOffset = monitor.getClientOffset();
      const hoverClientY = (clientOffset as XYCoord).y - hoverBoundingRect.top;

      // If we're hovering over a child don't trigger on the parent
      if (!monitor.isOver({ shallow: true })) {
        return;
      }

      // Don't show targets for dropping on self
      if (dragQualityIndex === dropQualityIndex) {
        return;
      }

      // Don't allow a group to be dropped inside a group
      if (isDragGroup && isDropGroupItem) {
        return;
      }

      let dropPosition: 'above' | 'below' | null = null;

      // Determine drop position based on position over target
      if (hoverClientY > hoverMiddleY) {
        dropPosition = 'below';
      } else if (hoverClientY < hoverMiddleY) {
        dropPosition = 'above';
      } else {
        return;
      }

      onDragMove({
        dragQualityIndex,
        dropQualityIndex,
        dropPosition,
      });
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
        height,
      };
    },
    collect: (monitor: DragSourceMonitor<unknown, unknown>) => ({
      isDragging: monitor.isDragging(),
    }),
    end: (_item: DragItem, monitor) => {
      onDragEnd(monitor.didDrop());
    },
  });

  dropRef(previewRef(ref));

  const isBefore = !isDragging && isDraggingUp && isOver;
  const isAfter = !isDragging && isDraggingDown && isOver;

  return (
    <div ref={ref} className={classNames(styles.qualityProfileItemDragSource)}>
      {isBefore ? (
        <div
          className={classNames(
            styles.qualityProfileItemPlaceholder,
            styles.qualityProfileItemPlaceholderBefore
          )}
          style={{
            height: dragHeight,
          }}
        />
      ) : null}

      <div ref={measureRef}>
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
            qualityId={qualityId}
            name={name}
            allowed={allowed}
            isDragging={isDragging}
          />
        ) : null}
      </div>

      {isAfter ? (
        <div
          className={classNames(
            styles.qualityProfileItemPlaceholder,
            styles.qualityProfileItemPlaceholderAfter
          )}
          style={{
            height: dragHeight,
          }}
        />
      ) : null}
    </div>
  );
}

export default QualityProfileItemDragSource;
