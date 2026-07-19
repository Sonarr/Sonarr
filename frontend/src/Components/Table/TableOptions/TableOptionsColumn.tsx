import { useSortable } from '@dnd-kit/react/sortable';
import classNames from 'classnames';
import React from 'react';
import CheckInput from 'Components/Form/CheckInput';
import Icon from 'Components/Icon';
import { icons } from 'Helpers/Props';
import { CheckInputChanged } from 'typings/inputs';
import Column, { IsModifiable } from '../Column';
import styles from './TableOptionsColumn.css';

interface TableOptionsColumnProps {
  name: string;
  label: Column['label'];
  isVisible: boolean;
  isModifiable: IsModifiable;
  index: number;
  onVisibleChange: (change: CheckInputChanged) => void;
}

function TableOptionsColumn({
  name,
  label,
  index,
  isVisible,
  isModifiable,
  onVisibleChange,
}: TableOptionsColumnProps) {
  const { ref, handleRef, isDragging } = useSortable({
    id: name,
    index,
    disabled: isModifiable === 'disabled',
  });

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
          {typeof label === 'function' ? label() : label}
        </label>

        {isModifiable === 'disabled' ? null : (
          <div ref={handleRef} className={styles.dragHandle}>
            <Icon name={icons.GRIP} size={16} />
          </div>
        )}
      </div>
    </div>
  );
}

export default TableOptionsColumn;
