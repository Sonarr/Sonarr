import React from 'react';
import { useSelector } from 'react-redux';
import Series from 'Series/Series';
import createAllSeriesSelector from 'Store/Selectors/createAllSeriesSelector';
import sortByProp from 'Utilities/Array/sortByProp';
import FilterBuilderRowValue, {
  FilterBuilderRowValueProps,
} from './FilterBuilderRowValue';

type SeriesFilterBuilderRowValueProps<T> = Omit<
  FilterBuilderRowValueProps<T, number>,
  'tagList'
>;

function SeriesFilterBuilderRowValue<T>(
  props: SeriesFilterBuilderRowValueProps<T>
) {
  const allSeries: Series[] = useSelector(createAllSeriesSelector());

  const tagList = allSeries
    .map((series) => ({ id: series.id, name: series.title }))
    .sort(sortByProp('name'));

  return <FilterBuilderRowValue {...props} tagList={tagList} />;
}

export default SeriesFilterBuilderRowValue;
