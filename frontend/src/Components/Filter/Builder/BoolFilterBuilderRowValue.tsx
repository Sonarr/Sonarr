import React from 'react';
import FilterBuilderRowValue, {
  FilterBuilderRowValueProps,
} from './FilterBuilderRowValue';

const protocols = [
  { id: true, name: 'true' },
  { id: false, name: 'false' },
];

type BoolFilterBuilderRowValueProps<T> = Omit<
  FilterBuilderRowValueProps<T, boolean>,
  'tagList'
>;

function BoolFilterBuilderRowValue<T>(
  props: BoolFilterBuilderRowValueProps<T>
) {
  return <FilterBuilderRowValue tagList={protocols} {...props} />;
}

export default BoolFilterBuilderRowValue;
