import React from 'react';
import { useSelector } from 'react-redux';
import Series from 'Series/Series';
import createAllSeriesSelector from 'Store/Selectors/createAllSeriesSelector';
import FilterBuilderRowValue from './FilterBuilderRowValue';
import FilterBuilderRowValueProps from './FilterBuilderRowValueProps';

function SeriesFilterBuilderRowValue(props: FilterBuilderRowValueProps) {
  const allSeries: Series[] = useSelector(createAllSeriesSelector());

  const tagList = allSeries.map((series) => {
    return {
      id: series.id,
      name: series.title,
    };
  });

  return <FilterBuilderRowValue {...props} tagList={tagList} />;
}

export default SeriesFilterBuilderRowValue;
