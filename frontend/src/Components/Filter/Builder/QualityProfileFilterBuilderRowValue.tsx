import React from 'react';
import { useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import sortByProp from 'Utilities/Array/sortByProp';
import FilterBuilderRowValue, {
  FilterBuilderRowValueProps,
} from './FilterBuilderRowValue';

function createQualityProfilesSelector() {
  return createSelector(
    (state: AppState) => state.settings.qualityProfiles.items,
    (qualityProfiles) => {
      return qualityProfiles;
    }
  );
}

type QualityProfileFilterBuilderRowValueProps<T> = Omit<
  FilterBuilderRowValueProps<T, number>,
  'tagList'
>;

function QualityProfileFilterBuilderRowValue<T>(
  props: QualityProfileFilterBuilderRowValueProps<T>
) {
  const qualityProfiles = useSelector(createQualityProfilesSelector());

  const tagList = qualityProfiles
    .map(({ id, name }) => ({ id, name }))
    .sort(sortByProp('name'));

  return <FilterBuilderRowValue {...props} tagList={tagList} />;
}

export default QualityProfileFilterBuilderRowValue;
