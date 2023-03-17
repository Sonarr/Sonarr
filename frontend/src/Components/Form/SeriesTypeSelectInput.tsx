import React from 'react';
import * as seriesTypes from 'Utilities/Series/seriesTypes';
import EnhancedSelectInput from './EnhancedSelectInput';
import SeriesTypeSelectInputOption from './SeriesTypeSelectInputOption';
import SeriesTypeSelectInputSelectedValue from './SeriesTypeSelectInputSelectedValue';

interface SeriesTypeSelectInputProps {
  includeNoChange: boolean;
  includeNoChangeDisabled?: boolean;
  includeMixed: boolean;
}

interface ISeriesTypeOption {
  key: string;
  value: string;
  format?: string;
  disabled?: boolean;
}

const seriesTypeOptions: ISeriesTypeOption[] = [
  {
    key: seriesTypes.STANDARD,
    value: 'Standard',
    format: 'Season and episode numbers (S01E05)',
  },
  {
    key: seriesTypes.DAILY,
    value: 'Daily / Date',
    format: 'Date (2020-05-25)',
  },
  {
    key: seriesTypes.ANIME,
    value: 'Anime / Absolute',
    format: 'Absolute episode Number (005)',
  },
];

function SeriesTypeSelectInput(props: SeriesTypeSelectInputProps) {
  const values = [...seriesTypeOptions];

  const {
    includeNoChange,
    includeNoChangeDisabled = true,
    includeMixed,
  } = props;

  if (includeNoChange) {
    values.unshift({
      key: 'noChange',
      value: 'No Change',
      disabled: includeNoChangeDisabled,
    });
  }

  if (includeMixed) {
    values.unshift({
      key: 'mixed',
      value: '(Mixed)',
      disabled: true,
    });
  }

  return (
    <EnhancedSelectInput
      {...props}
      values={values}
      optionComponent={SeriesTypeSelectInputOption}
      selectedValueComponent={SeriesTypeSelectInputSelectedValue}
    />
  );
}

SeriesTypeSelectInput.defaultProps = {
  includeNoChange: false,
  includeMixed: false,
};

export default SeriesTypeSelectInput;
