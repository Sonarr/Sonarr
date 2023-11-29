import React from 'react';
import translate from 'Utilities/String/translate';
import FilterBuilderRowValue from './FilterBuilderRowValue';
import FilterBuilderRowValueProps from './FilterBuilderRowValueProps';

const EVENT_TYPE_OPTIONS = [
  {
    id: 1,
    get name() {
      return translate('Grabbed');
    },
  },
  {
    id: 3,
    get name() {
      return translate('Imported');
    },
  },
  {
    id: 4,
    get name() {
      return translate('Failed');
    },
  },
  {
    id: 5,
    get name() {
      return translate('Deleted');
    },
  },
  {
    id: 6,
    get name() {
      return translate('Renamed');
    },
  },
  {
    id: 7,
    get name() {
      return translate('Ignored');
    },
  },
];

function HistoryEventTypeFilterBuilderRowValue(
  props: FilterBuilderRowValueProps
) {
  return <FilterBuilderRowValue {...props} tagList={EVENT_TYPE_OPTIONS} />;
}

export default HistoryEventTypeFilterBuilderRowValue;
