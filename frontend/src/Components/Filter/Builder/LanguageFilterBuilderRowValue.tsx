import React from 'react';
import { useLanguages } from 'Language/useLanguages';
import FilterBuilderRowValue, {
  FilterBuilderRowValueProps,
} from './FilterBuilderRowValue';

type LanguageFilterBuilderRowValueProps<T> = Omit<
  FilterBuilderRowValueProps<T, number, string>,
  'tagList'
>;

function LanguageFilterBuilderRowValue<T>(
  props: LanguageFilterBuilderRowValueProps<T>
) {
  const { data: items = [] } = useLanguages();

  return <FilterBuilderRowValue {...props} tagList={items} />;
}

export default LanguageFilterBuilderRowValue;
