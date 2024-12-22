import React from 'react';
import translate from 'Utilities/String/translate';
import FilterBuilderRowValue, {
  FilterBuilderRowValueProps,
} from './FilterBuilderRowValue';

const statusTagList = [
  {
    id: 'continuing',
    get name() {
      return translate('Continuing');
    },
  },
  {
    id: 'upcoming',
    get name() {
      return translate('Upcoming');
    },
  },
  {
    id: 'ended',
    get name() {
      return translate('Ended');
    },
  },
  {
    id: 'deleted',
    get name() {
      return translate('Deleted');
    },
  },
];

type SeriesStatusFilterBuilderRowValueProps<T> = Omit<
  FilterBuilderRowValueProps<T, string>,
  'tagList'
>;

function SeriesStatusFilterBuilderRowValue<T>(
  props: SeriesStatusFilterBuilderRowValueProps<T>
) {
  return <FilterBuilderRowValue tagList={statusTagList} {...props} />;
}

export default SeriesStatusFilterBuilderRowValue;
