import React, { useMemo } from 'react';
import * as seriesEditions from 'Utilities/Series/seriesEditions';
import translate from 'Utilities/String/translate';
import EnhancedSelectInput, {
  EnhancedSelectInputProps,
  EnhancedSelectInputValue,
} from './EnhancedSelectInput';

export interface SeriesEditionSelectInputProps
  extends Omit<
    EnhancedSelectInputProps<EnhancedSelectInputValue<string>, string>,
    'values'
  > {
  includeNoChange?: boolean;
  includeNoChangeDisabled?: boolean;
  includeMixed?: boolean;
}

interface SeriesEditionOption {
  key: string;
  value: string;
  isDisabled?: boolean;
}

const seriesEditionOptions: SeriesEditionOption[] = [
  {
    key: seriesEditions.STANDARD,
    value: 'Standard',
  },
  {
    key: seriesEditions.DIRECTORS_CUT,
    value: "Director's Cut",
  },
  {
    key: seriesEditions.CUSTOM,
    value: 'Custom',
  },
];

function SeriesEditionSelectInput(props: SeriesEditionSelectInputProps) {
  const {
    includeNoChange = false,
    includeNoChangeDisabled = true,
    includeMixed = false,
  } = props;

  const values = useMemo(() => {
    const result = [...seriesEditionOptions];

    if (includeNoChange) {
      result.unshift({
        key: 'noChange',
        value: translate('NoChange'),
        isDisabled: includeNoChangeDisabled,
      });
    }

    if (includeMixed) {
      result.unshift({
        key: 'mixed',
        value: `(${translate('Mixed')})`,
        isDisabled: true,
      });
    }

    return result;
  }, [includeNoChange, includeNoChangeDisabled, includeMixed]);

  return <EnhancedSelectInput {...props} values={values} />;
}

export default SeriesEditionSelectInput;
