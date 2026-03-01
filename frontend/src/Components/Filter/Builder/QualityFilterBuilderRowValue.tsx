import React, { useMemo } from 'react';
import { useQualityProfileSchema } from 'Settings/Profiles/Quality/useQualityProfiles';
import getQualities from 'Utilities/Quality/getQualities';
import FilterBuilderRowValue, {
  FilterBuilderRowValueProps,
} from './FilterBuilderRowValue';

type QualityFilterBuilderRowValueProps<T> = Omit<
  FilterBuilderRowValueProps<T, number, string>,
  'tagList'
>;

function QualityFilterBuilderRowValue<T>(
  props: QualityFilterBuilderRowValueProps<T>
) {
  const { schema } = useQualityProfileSchema(true);

  const tagList = useMemo(() => {
    return getQualities(schema.items);
  }, [schema]);

  return <FilterBuilderRowValue {...props} tagList={tagList} />;
}

export default QualityFilterBuilderRowValue;
