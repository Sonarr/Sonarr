import React from 'react';
import FilterBuilderRowValue from './FilterBuilderRowValue';

const protocols = [
  { id: 'continuing', name: 'Continuing' },
  { id: 'ended', name: 'Ended' }
];

function SeriesStatusFilterBuilderRowValue(props) {
  return (
    <FilterBuilderRowValue
      tagList={protocols}
      {...props}
    />
  );
}

export default SeriesStatusFilterBuilderRowValue;
