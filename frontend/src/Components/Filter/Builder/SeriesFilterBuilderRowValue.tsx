import React from 'react';
import useSeries from 'Series/useSeries';
import sortByProp from 'Utilities/Array/sortByProp';
import FilterBuilderRowValue, {
  FilterBuilderRowValueProps,
} from './FilterBuilderRowValue';

type SeriesFilterBuilderRowValueProps<T> = Omit<
  FilterBuilderRowValueProps<T, number, string>,
  'tagList'
>;

function SeriesFilterBuilderRowValue<T>(
  props: SeriesFilterBuilderRowValueProps<T>
) {
  const { data: allSeries = [] } = useSeries();

  const tagList = allSeries
    .map((series) => ({ id: series.id, name: series.title }))
    .sort(sortByProp('name'));

  return <FilterBuilderRowValue {...props} tagList={tagList} />;
}

export default SeriesFilterBuilderRowValue;
