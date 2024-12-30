import classNames from 'classnames';
import React, { useRef } from 'react';
import { DragSourceMonitor, useDrag, useDrop, XYCoord } from 'react-dnd';
import CheckInput from 'Components/Form/CheckInput';
import Icon from 'Components/Icon';
import DragType from 'Helpers/DragType';
import { icons } from 'Helpers/Props';
import { CheckInputChanged } from 'typings/inputs';
import Column from '../Column';
import styles from './TableOptionsColumn.css';

interface DragItem {
  name: string;
  index: number;
}

interface TableOptionsColumnProps {
  name: string;
  label: Column['label'];
  isDraggingDown: boolean;
  isDraggingUp: boolean;
  isVisible: boolean;
  isModifiable: boolean;
  index: number;
  onVisibleChange: (change: CheckInputChanged) => void;
  onColumnDragEnd: (didDrop: boolean) => void;
  onColumnDragMove: (dragIndex: number, hoverIndex: number) => void;
}

function TableOptionsColumn({
  name,
  label,
  index,
  isDraggingDown,
  isDraggingUp,
  isVisible,
  isModifiable,
  onVisibleChange,
  onColumnDragEnd,
  onColumnDragMove,
}: TableOptionsColumnProps) {
  const ref = useRef<HTMLDivElement>(null);

  const [{ isOver }, dropRef] = useDrop<DragItem, void, { isOver: boolean }>({
    accept: DragType.TableColumn,
    collect(monitor) {
      return {
        isOver: monitor.isOver(),
      };
    },
    hover(item: DragItem, monitor) {
      if (!ref.current) {
        return;
      }

      if (!isModifiable) {
        return;
      }

      const dragIndex = item.index;
      const hoverIndex = index;

      // Don't replace items with themselves
      if (dragIndex === hoverIndex) {
        return;
      }

      // Determine rectangle on screen
      const hoverBoundingRect = ref.current?.getBoundingClientRect();

      // Get vertical middle
      const hoverMiddleY =
        (hoverBoundingRect.bottom - hoverBoundingRect.top) / 2;

      // Determine mouse position
      const clientOffset = monitor.getClientOffset();

      // Get pixels to the top
      const hoverClientY = (clientOffset as XYCoord).y - hoverBoundingRect.top;

      // When moving up, only trigger if drag position is above 50% and
      // when moving down, only trigger if drag position is below 50%.
      // If we're moving down the hoverIndex needs to be increased
      // by one so it's ordered properly. Otherwise the hoverIndex will work.

      // Dragging downwards
      if (dragIndex < hoverIndex && hoverClientY < hoverMiddleY) {
        return;
      }

      // Dragging upwards
      if (dragIndex > hoverIndex && hoverClientY > hoverMiddleY) {
        return;
      }

      onColumnDragMove(dragIndex, hoverIndex);
    },
  });

  const [{ isDragging }, dragRef, previewRef] = useDrag<
    DragItem,
    unknown,
    { isDragging: boolean }
  >({
    type: DragType.TableColumn,
    item: () => {
      return {
        name,
        index,
      };
    },
    collect: (monitor: DragSourceMonitor<unknown, unknown>) => ({
      isDragging: monitor.isDragging(),
    }),
    end: (_item: DragItem, monitor) => {
      onColumnDragEnd(monitor.didDrop());
    },
  });

  dropRef(previewRef(ref));

  const isBefore = !isDragging && isDraggingUp && isOver;
  const isAfter = !isDragging && isDraggingDown && isOver;

  return (
    <div ref={ref} className={styles.columnContainer}>
      {isBefore ? (
        <div
          className={classNames(styles.placeholder, styles.placeholderBefore)}
        />
      ) : null}

      <div
        className={classNames(styles.column, isDragging && styles.isDragging)}
      >
        <label className={styles.label}>
          <CheckInput
            containerClassName={styles.checkContainer}
            name={name}
            value={isVisible}
            isDisabled={isModifiable === false}
            onChange={onVisibleChange}
          />
          {typeof label === 'function' ? label() : label}
        </label>

        {isModifiable ? (
          <div ref={dragRef} className={styles.dragHandle}>
            <Icon className={styles.dragIcon} name={icons.REORDER} />
          </div>
        ) : null}
      </div>

      {isAfter ? (
        <div
          className={classNames(styles.placeholder, styles.placeholderAfter)}
        />
      ) : null}
    </div>
  );
}

export default TableOptionsColumn;
