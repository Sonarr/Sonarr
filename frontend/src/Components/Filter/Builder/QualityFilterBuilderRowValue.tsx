import React, { useEffect, useMemo } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import { fetchQualityProfileSchema } from 'Store/Actions/settingsActions';
import getQualities from 'Utilities/Quality/getQualities';
import FilterBuilderRowValue, {
  FilterBuilderRowValueProps,
} from './FilterBuilderRowValue';

type QualityFilterBuilderRowValueProps<T> = Omit<
  FilterBuilderRowValueProps<T, number>,
  'tagList'
>;

function QualityFilterBuilderRowValue<T>(
  props: QualityFilterBuilderRowValueProps<T>
) {
  const dispatch = useDispatch();

  const { isSchemaPopulated, schema } = useSelector(
    (state: AppState) => state.settings.qualityProfiles
  );

  const tagList = useMemo(() => {
    return getQualities(schema.items);
  }, [schema]);

  useEffect(() => {
    if (!isSchemaPopulated) {
      dispatch(fetchQualityProfileSchema());
    }
  }, [isSchemaPopulated, dispatch]);

  return <FilterBuilderRowValue {...props} tagList={tagList} />;
}

export default QualityFilterBuilderRowValue;
