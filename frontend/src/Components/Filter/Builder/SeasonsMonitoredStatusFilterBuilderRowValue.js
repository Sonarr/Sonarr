import React from 'react';
import translate from 'Utilities/String/translate';
import FilterBuilderRowValue from './FilterBuilderRowValue';

const seasonsMonitoredStatusList = [
  {
    id: 'all',
    get name() {
      return translate('SeasonsMonitoredAll');
    }
  },
  {
    id: 'partial',
    get name() {
      return translate('SeasonsMonitoredPartial');
    }
  },
  {
    id: 'none',
    get name() {
      return translate('SeasonsMonitoredNone');
    }
  }
];

function SeasonsMonitoredStatusFilterBuilderRowValue(props) {
  return (
    <FilterBuilderRowValue
      tagList={seasonsMonitoredStatusList}
      {...props}
    />
  );
}

export default SeasonsMonitoredStatusFilterBuilderRowValue;
