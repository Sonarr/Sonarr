import React, { useMemo } from 'react';
import { useSelector } from 'react-redux';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import FilterBuilderRowValue, {
  FilterBuilderRowValueProps,
} from './FilterBuilderRowValue';

type TagFilterBuilderRowValueProps<T> = Omit<
  FilterBuilderRowValueProps<T, number>,
  'tagList'
>;

function TagFilterBuilderRowValue<T>(props: TagFilterBuilderRowValueProps<T>) {
  const tags = useSelector(createTagsSelector());

  const tagList = useMemo(() => {
    return tags.map((tag) => {
      const { id, label } = tag;

      return {
        id,
        name: label,
      };
    });
  }, [tags]);

  return <FilterBuilderRowValue {...props} tagList={tagList} />;
}

export default TagFilterBuilderRowValue;
