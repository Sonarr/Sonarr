import React from 'react';
import translate from 'Utilities/String/translate';
import FilterBuilderRowValue from './FilterBuilderRowValue';

const seriesTypeList = [
  {
    id: 'anime',
    get name() {
      return translate('Anime');
    }
  },
  {
    id: 'daily',
    get name() {
      return translate('Daily');
    }
  },
  {
    id: 'standard',
    get name() {
      return translate('Standard');
    }
  }
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
