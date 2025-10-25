import React, { useCallback } from 'react';
import CheckInput from 'Components/Form/CheckInput';
import { CheckInputChanged } from 'typings/inputs';
import { SelectStateInputProps } from 'typings/props';
import VirtualTableRowCell, {
  VirtualTableRowCellProps,
} from './VirtualTableRowCell';
import styles from './VirtualTableSelectCell.css';

interface VirtualTableSelectCellProps<T extends number | string = number>
  extends VirtualTableRowCellProps {
  inputClassName?: string;
  id: T;
  isSelected?: boolean;
  isDisabled: boolean;
  onSelectedChange: (options: SelectStateInputProps<T>) => void;
}

function VirtualTableSelectCell<T extends number | string = number>({
  inputClassName = styles.input,
  id,
  isSelected = false,
  isDisabled,
  onSelectedChange,
  ...otherProps
}: VirtualTableSelectCellProps<T>) {
  const handleChange = useCallback(
    ({ value, shiftKey }: CheckInputChanged) => {
      onSelectedChange({ id, value, shiftKey });
    },
    [id, onSelectedChange]
  );

  return (
    <VirtualTableRowCell className={styles.cell} {...otherProps}>
      <CheckInput
        className={inputClassName}
        name={id.toString()}
        value={isSelected}
        isDisabled={isDisabled}
        onChange={handleChange}
      />
    </VirtualTableRowCell>
  );
}

export default VirtualTableSelectCell;
