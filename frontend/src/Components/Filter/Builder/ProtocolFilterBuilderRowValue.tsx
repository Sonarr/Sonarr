import React from 'react';
import FilterBuilderRowValue, {
  FilterBuilderRowValueProps,
} from './FilterBuilderRowValue';

const protocols = [
  { id: 'torrent', name: 'Torrent' },
  { id: 'usenet', name: 'Usenet' },
];

type ProtocolFilterBuilderRowValueProps<T> = Omit<
  FilterBuilderRowValueProps<T, string>,
  'tagList'
>;

function ProtocolFilterBuilderRowValue<T>(
  props: ProtocolFilterBuilderRowValueProps<T>
) {
  return <FilterBuilderRowValue tagList={protocols} {...props} />;
}

export default ProtocolFilterBuilderRowValue;
