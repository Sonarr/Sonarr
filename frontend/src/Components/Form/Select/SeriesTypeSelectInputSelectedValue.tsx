import React from 'react';
import HintedSelectInputSelectedValue from './HintedSelectInputSelectedValue';
import { ISeriesTypeOption } from './SeriesTypeSelectInput';

interface SeriesTypeSelectInputOptionProps {
  selectedValue: string;
  values: ISeriesTypeOption[];
  format: string;
}
function SeriesTypeSelectInputSelectedValue(
  props: SeriesTypeSelectInputOptionProps
) {
  const { selectedValue, values, ...otherProps } = props;
  const format = values.find((v) => v.key === selectedValue)?.format;

  return (
    <HintedSelectInputSelectedValue
      {...otherProps}
      selectedValue={selectedValue}
      values={values}
      hint={format}
    />
  );
}

export default SeriesTypeSelectInputSelectedValue;
