import { useSortable } from '@dnd-kit/react/sortable';
import classNames from 'classnames';
import React from 'react';
import CheckInput from 'Components/Form/CheckInput';
import SortableHandle, {
  SortableHandleOption,
} from 'Components/Sortable/SortableHandle';
import { CheckInputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import Column, { IsModifiable } from '../Column';
import styles from './TableOptionsColumn.module.css';

interface TableOptionsColumnProps {
  name: string;
  label: Column['label'];
  isVisible: boolean;
  isModifiable: IsModifiable;
  index: number;
  isFirst: boolean;
  isLast: boolean;
  onVisibleChange: (change: CheckInputChanged) => void;
  onMove: (name: string, offset: number) => void;
}

function TableOptionsColumn({
  name,
  label,
  index,
  isVisible,
  isModifiable,
  isFirst,
  isLast,
  onVisibleChange,
  onMove,
}: TableOptionsColumnProps) {
  const { ref, handleRef, isDragging } = useSortable({
    id: name,
    index,
    disabled: isModifiable === 'disabled',
  });

  const resolvedLabel = typeof label === 'function' ? label() : label;
  const itemLabel = typeof resolvedLabel === 'string' ? resolvedLabel : name;
  const moveOptions: SortableHandleOption[] = [
    {
      isDisabled: isFirst,
      label: translate('MoveItemUp', { item: itemLabel }),
      onPress: () => onMove(name, -1),
    },
    {
      isDisabled: isLast,
      label: translate('MoveItemDown', { item: itemLabel }),
      onPress: () => onMove(name, 1),
    },
  ];

  return (
    <div ref={ref} className={styles.columnContainer}>
      <div
        className={classNames(
          styles.column,
          isDragging && styles.isDragging,
          !isVisible && styles.hidden
        )}
      >
        <label className={styles.label}>
          <CheckInput
            containerClassName={styles.checkContainer}
            name={name}
            value={isVisible}
            isDisabled={isModifiable !== 'enabled'}
            onChange={onVisibleChange}
          />
          {resolvedLabel}
        </label>

        {isModifiable === 'disabled' ? null : (
          <SortableHandle
            className={styles.dragHandle}
            handleRef={handleRef}
            label={itemLabel}
            options={moveOptions}
          />
        )}
      </div>
    </div>
  );
}

export default TableOptionsColumn;
