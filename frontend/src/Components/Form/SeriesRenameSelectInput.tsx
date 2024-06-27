import React from 'react';
import * as seriesRenameTypes from 'Utilities/Series/seriesRenameTypes';
import translate from 'Utilities/String/translate';
import EnhancedSelectInput from './EnhancedSelectInput';

interface SeriesRenameSelectInputProps {
  includeNoChange: boolean;
  includeNoChangeDisabled?: boolean;
  includeMixed: boolean;
}

interface ISeriesRenameOption {
  key: string;
  value: string;
  isDisabled?: boolean;
}

const seriesRenameOptions: ISeriesRenameOption[] = [
  {
    key: seriesRenameTypes.SYSTEM,
    get value() {
      return translate('System');
    },
  },
  {
    key: seriesRenameTypes.DISABLED,
    get value() {
      return translate('Disabled');
    },
  },
  {
    key: seriesRenameTypes.ENABLED,
    get value() {
      return translate('Enabled');
    },
  },
];

function SeriesRenameSelectInput(props: SeriesRenameSelectInputProps) {
  const values = [...seriesRenameOptions];

  const {
    includeNoChange,
    includeNoChangeDisabled = true,
    includeMixed,
  } = props;

  if (includeNoChange) {
    values.unshift({
      key: 'noChange',
      value: translate('NoChange'),
      isDisabled: includeNoChangeDisabled,
    });
  }

  if (includeMixed) {
    values.unshift({
      key: 'mixed',
      value: `(${translate('Mixed')})`,
      isDisabled: true,
    });
  }

  return <EnhancedSelectInput {...props} values={values} />;
}

SeriesRenameSelectInput.defaultProps = {
  includeNoChange: false,
  includeMixed: false,
};

export default SeriesRenameSelectInput;
