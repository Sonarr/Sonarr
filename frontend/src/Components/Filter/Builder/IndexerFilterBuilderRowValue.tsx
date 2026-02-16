import React, { useMemo } from 'react';
import { useSortedIndexers } from 'Settings/Indexers/useIndexers';
import FilterBuilderRowValue, {
  FilterBuilderRowValueProps,
} from './FilterBuilderRowValue';

type IndexerFilterBuilderRowValueProps<T> = Omit<
  FilterBuilderRowValueProps<T, number, string>,
  'tagList'
>;

function IndexerFilterBuilderRowValue<T>(
  props: IndexerFilterBuilderRowValueProps<T>
) {
  const { data } = useSortedIndexers();

  const tagList = useMemo(() => {
    return data.map((item) => {
      return {
        id: item.id,
        name: item.name,
      };
    });
  }, [data]);

  return <FilterBuilderRowValue {...props} tagList={tagList} />;
}

export default IndexerFilterBuilderRowValue;
