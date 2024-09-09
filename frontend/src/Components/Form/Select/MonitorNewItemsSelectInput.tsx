import React from 'react';
import monitorNewItemsOptions from 'Utilities/Series/monitorNewItemsOptions';
import EnhancedSelectInput, {
  EnhancedSelectInputProps,
  EnhancedSelectInputValue,
} from './EnhancedSelectInput';

interface MonitorNewItemsSelectInputProps
  extends Omit<
    EnhancedSelectInputProps<EnhancedSelectInputValue<string>, string>,
    'values'
  > {
  includeNoChange?: boolean;
  includeMixed?: boolean;
  onChange: (...args: unknown[]) => unknown;
}

function MonitorNewItemsSelectInput(props: MonitorNewItemsSelectInputProps) {
  const {
    includeNoChange = false,
    includeMixed = false,
    ...otherProps
  } = props;

  const values: EnhancedSelectInputValue<string>[] = [
    ...monitorNewItemsOptions,
  ];

  if (includeNoChange) {
    values.unshift({
      key: 'noChange',
      value: 'No Change',
      isDisabled: true,
    });
  }

  if (includeMixed) {
    values.unshift({
      key: 'mixed',
      value: '(Mixed)',
      isDisabled: true,
    });
  }

  return <EnhancedSelectInput {...otherProps} values={values} />;
}

export default MonitorNewItemsSelectInput;
