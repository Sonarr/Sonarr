import React, { useCallback } from 'react';
import CheckInput from 'Components/Form/CheckInput';
import { CheckInputChanged } from 'typings/inputs';
import { SelectStateInputProps } from 'typings/props';
import VirtualTableRowCell, {
  VirtualTableRowCellProps,
} from './VirtualTableRowCell';
import styles from './VirtualTableSelectCell.css';

interface VirtualTableSelectCellProps extends VirtualTableRowCellProps {
  inputClassName?: string;
  id: number | string;
  isSelected?: boolean;
  isDisabled: boolean;
  onSelectedChange: (options: SelectStateInputProps) => void;
}

function VirtualTableSelectCell({
  inputClassName = styles.input,
  id,
  isSelected = false,
  isDisabled,
  onSelectedChange,
  ...otherProps
}: VirtualTableSelectCellProps) {
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
