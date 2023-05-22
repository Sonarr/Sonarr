import React from 'react';
import { useSelector } from 'react-redux';
import createLanguagesSelector from 'Store/Selectors/createLanguagesSelector';
import FilterBuilderRowValue from './FilterBuilderRowValue';
import FilterBuilderRowValueProps from './FilterBuilderRowValueProps';

function LanguageFilterBuilderRowValue(props: FilterBuilderRowValueProps) {
  const { items } = useSelector(createLanguagesSelector());

  return <FilterBuilderRowValue {...props} tagList={items} />;
}

export default LanguageFilterBuilderRowValue;
