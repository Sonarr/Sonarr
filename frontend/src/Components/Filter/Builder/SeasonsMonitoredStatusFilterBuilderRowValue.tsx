import React from 'react';
import translate from 'Utilities/String/translate';
import FilterBuilderRowValue, {
  FilterBuilderRowValueProps,
} from './FilterBuilderRowValue';

const seasonsMonitoredStatusList = [
  {
    id: 'all',
    get name() {
      return translate('SeasonsMonitoredAll');
    },
  },
  {
    id: 'partial',
    get name() {
      return translate('SeasonsMonitoredPartial');
    },
  },
  {
    id: 'none',
    get name() {
      return translate('SeasonsMonitoredNone');
    },
  },
];

type SeasonsMonitoredStatusFilterBuilderRowValueProps<T> = Omit<
  FilterBuilderRowValueProps<T, string>,
  'tagList'
>;

function SeasonsMonitoredStatusFilterBuilderRowValue<T>(
  props: SeasonsMonitoredStatusFilterBuilderRowValueProps<T>
) {
  return (
    <FilterBuilderRowValue tagList={seasonsMonitoredStatusList} {...props} />
  );
}

export default SeasonsMonitoredStatusFilterBuilderRowValue;
