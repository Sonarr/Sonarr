import React from 'react';
import FilterBuilderRowValue from './FilterBuilderRowValue';

const seriesTypeList = [
  { id: 'anime', name: 'Anime' },
  { id: 'daily', name: 'Daily' },
  { id: 'standard', name: 'Standard' }
];

function SeriesTypeFilterBuilderRowValue(props) {
  return (
    <FilterBuilderRowValue
      tagList={seriesTypeList}
      {...props}
    />
  );
}

export default SeriesTypeFilterBuilderRowValue;
