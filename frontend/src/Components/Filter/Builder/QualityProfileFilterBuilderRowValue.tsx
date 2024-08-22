import React from 'react';
import { useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import FilterBuilderRowValueProps from 'Components/Filter/Builder/FilterBuilderRowValueProps';
import sortByProp from 'Utilities/Array/sortByProp';
import FilterBuilderRowValue from './FilterBuilderRowValue';

function createQualityProfilesSelector() {
  return createSelector(
    (state: AppState) => state.settings.qualityProfiles.items,
    (qualityProfiles) => {
      return qualityProfiles;
    }
  );
}

function QualityProfileFilterBuilderRowValue(
  props: FilterBuilderRowValueProps
) {
  const qualityProfiles = useSelector(createQualityProfilesSelector());

  const tagList = qualityProfiles
    .map(({ id, name }) => ({ id, name }))
    .sort(sortByProp('name'));

  return <FilterBuilderRowValue {...props} tagList={tagList} />;
}

export default QualityProfileFilterBuilderRowValue;
