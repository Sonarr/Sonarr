import React from 'react';
import monitorNewItemsOptions from 'Utilities/Series/monitorNewItemsOptions';
import EnhancedSelectInput, {
  EnhancedSelectInputProps,
  EnhancedSelectInputValue,
} from './EnhancedSelectInput';

export interface MonitorNewItemsSelectInputProps
  extends Omit<
    EnhancedSelectInputProps<EnhancedSelectInputValue<string>, string>,
    'values'
  > {
  includeNoChange?: boolean;
  includeNoChangeDisabled?: boolean;
  includeMixed?: boolean;
}

function MonitorNewItemsSelectInput(props: MonitorNewItemsSelectInputProps) {
  const {
    includeNoChange = false,
    includeNoChangeDisabled = true,
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
      isDisabled: includeNoChangeDisabled,
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
