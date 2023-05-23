import React from 'react';
import FilterBuilderRowValue from './FilterBuilderRowValue';
import FilterBuilderRowValueProps from './FilterBuilderRowValueProps';

const EVENT_TYPE_OPTIONS = [
  {
    id: 1,
    name: 'Grabbed',
  },
  {
    id: 3,
    name: 'Imported',
  },
  {
    id: 4,
    name: 'Failed',
  },
  {
    id: 5,
    name: 'Deleted',
  },
  {
    id: 6,
    name: 'Renamed',
  },
  {
    id: 7,
    name: 'Ignored',
  },
];

function HistoryEventTypeFilterBuilderRowValue(
  props: FilterBuilderRowValueProps
) {
  return <FilterBuilderRowValue {...props} tagList={EVENT_TYPE_OPTIONS} />;
}

export default HistoryEventTypeFilterBuilderRowValue;
