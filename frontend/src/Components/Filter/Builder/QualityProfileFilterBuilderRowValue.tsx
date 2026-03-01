import React from 'react';
import { useQualityProfilesData } from 'Settings/Profiles/Quality/useQualityProfiles';
import sortByProp from 'Utilities/Array/sortByProp';
import FilterBuilderRowValue, {
  FilterBuilderRowValueProps,
} from './FilterBuilderRowValue';

type QualityProfileFilterBuilderRowValueProps<T> = Omit<
  FilterBuilderRowValueProps<T, number, string>,
  'tagList'
>;

function QualityProfileFilterBuilderRowValue<T>(
  props: QualityProfileFilterBuilderRowValueProps<T>
) {
  const qualityProfiles = useQualityProfilesData();

  const tagList = qualityProfiles
    .map(({ id, name }) => ({ id, name }))
    .sort(sortByProp('name'));

  return <FilterBuilderRowValue {...props} tagList={tagList} />;
}

export default QualityProfileFilterBuilderRowValue;
