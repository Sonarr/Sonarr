import React from 'react';
import translate from 'Utilities/String/translate';
import FilterBuilderRowValue, {
  FilterBuilderRowValueProps,
} from './FilterBuilderRowValue';

const monitoredStatusList = [
  {
    id: 'all',
    get name() {
      return translate('MonitoredAll');
    },
  },
  {
    id: 'partial',
    get name() {
      return translate('MonitoredPartial');
    },
  },
  {
    id: 'none',
    get name() {
      return translate('MonitoredNone');
    },
  },
];

type MonitoredStatusFilterBuilderRowValueProps<T> = Omit<
  FilterBuilderRowValueProps<T, string, string>,
  'tagList'
>;

function MonitoredStatusFilterBuilderRowValue<T>(
  props: MonitoredStatusFilterBuilderRowValueProps<T>
) {
  return <FilterBuilderRowValue tagList={monitoredStatusList} {...props} />;
}

export default MonitoredStatusFilterBuilderRowValue;
