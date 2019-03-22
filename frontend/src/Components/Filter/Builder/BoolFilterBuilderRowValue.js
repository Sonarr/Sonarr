import React from 'react';
import FilterBuilderRowValue from './FilterBuilderRowValue';

const protocols = [
  { id: true, name: 'true' },
  { id: false, name: 'false' }
];

function BoolFilterBuilderRowValue(props) {
  return (
    <FilterBuilderRowValue
      tagList={protocols}
      {...props}
    />
  );
}

export default BoolFilterBuilderRowValue;
