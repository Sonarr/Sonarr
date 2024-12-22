import React from 'react';
import translate from 'Utilities/String/translate';
import FilterBuilderRowValue, {
  FilterBuilderRowValueProps,
} from './FilterBuilderRowValue';

const seriesTypeList = [
  {
    id: 'anime',
    get name() {
      return translate('Anime');
    },
  },
  {
    id: 'daily',
    get name() {
      return translate('Daily');
    },
  },
  {
    id: 'standard',
    get name() {
      return translate('Standard');
    },
  },
];

type SeriesTypeFilterBuilderRowValueProps<T> = Omit<
  FilterBuilderRowValueProps<T, string>,
  'tagList'
>;

function SeriesTypeFilterBuilderRowValue<T>(
  props: SeriesTypeFilterBuilderRowValueProps<T>
) {
  return <FilterBuilderRowValue tagList={seriesTypeList} {...props} />;
}

export default SeriesTypeFilterBuilderRowValue;
