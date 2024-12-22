import React from 'react';
import { useSelector } from 'react-redux';
import createLanguagesSelector from 'Store/Selectors/createLanguagesSelector';
import FilterBuilderRowValue, {
  FilterBuilderRowValueProps,
} from './FilterBuilderRowValue';

type LanguageFilterBuilderRowValueProps<T> = Omit<
  FilterBuilderRowValueProps<T, number>,
  'tagList'
>;

function LanguageFilterBuilderRowValue<T>(
  props: LanguageFilterBuilderRowValueProps<T>
) {
  const { items } = useSelector(createLanguagesSelector());

  return <FilterBuilderRowValue {...props} tagList={items} />;
}

export default LanguageFilterBuilderRowValue;
