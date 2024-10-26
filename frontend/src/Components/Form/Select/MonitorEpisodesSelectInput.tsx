import React from 'react';
import monitorOptions from 'Utilities/Series/monitorOptions';
import translate from 'Utilities/String/translate';
import EnhancedSelectInput, {
  EnhancedSelectInputProps,
  EnhancedSelectInputValue,
} from './EnhancedSelectInput';

interface MonitorEpisodesSelectInputProps
  extends Omit<
    EnhancedSelectInputProps<EnhancedSelectInputValue<string>, string>,
    'values'
  > {
  includeNoChange: boolean;
  includeMixed: boolean;
}

function MonitorEpisodesSelectInput(props: MonitorEpisodesSelectInputProps) {
  const {
    includeNoChange = false,
    includeMixed = false,
    ...otherProps
  } = props;

  const values: EnhancedSelectInputValue<string>[] = [...monitorOptions];

  if (includeNoChange) {
    values.unshift({
      key: 'noChange',
      get value() {
        return translate('NoChange');
      },
      isDisabled: true,
    });
  }

  if (includeMixed) {
    values.unshift({
      key: 'mixed',
      get value() {
        return `(${translate('Mixed')})`;
      },
      isDisabled: true,
    });
  }

  return <EnhancedSelectInput {...otherProps} values={values} />;
}

export default MonitorEpisodesSelectInput;
