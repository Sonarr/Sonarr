import React from 'react';
import { useSelector } from 'react-redux';
import Series from 'Series/Series';
import createAllSeriesSelector from 'Store/Selectors/createAllSeriesSelector';
import sortByProp from 'Utilities/Array/sortByProp';
import FilterBuilderRowValue from './FilterBuilderRowValue';
import FilterBuilderRowValueProps from './FilterBuilderRowValueProps';

function SeriesFilterBuilderRowValue(props: FilterBuilderRowValueProps) {
  const allSeries: Series[] = useSelector(createAllSeriesSelector());

  const tagList = allSeries
    .map((series) => ({ id: series.id, name: series.title }))
    .sort(sortByProp('name'));

  return <FilterBuilderRowValue {...props} tagList={tagList} />;
}

export default SeriesFilterBuilderRowValue;
