import React from 'react';
import FilterBuilderRowValue from './FilterBuilderRowValue';

const seriesStatusList = [
  { id: 'continuing', name: 'Continuing' },
  { id: 'upcoming', name: 'Upcoming' },
  { id: 'ended', name: 'Ended' },
  { id: 'deleted', name: 'Deleted' }
];

function SeriesStatusFilterBuilderRowValue(props) {
  return (
    <FilterBuilderRowValue
      tagList={seriesStatusList}
      {...props}
    />
  );
}

export default SeriesStatusFilterBuilderRowValue;
