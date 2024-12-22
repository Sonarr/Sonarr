import React, { useCallback, useMemo } from 'react';
import { FilterBuilderProp } from 'App/State/AppState';
import TagInput, { TagBase } from 'Components/Form/Tag/TagInput';
import {
  filterBuilderTypes,
  filterBuilderValueTypes,
  kinds,
} from 'Helpers/Props';
import { InputOnChange } from 'typings/inputs';
import convertToBytes from 'Utilities/Number/convertToBytes';
import formatBytes from 'Utilities/Number/formatBytes';
import FilterBuilderRowValueTag from './FilterBuilderRowValueTag';

export const NAME = 'value';

function getTagDisplayValue<T>(
  value: string | number | boolean,
  selectedFilterBuilderProp: FilterBuilderProp<T>
) {
  if (
    (typeof value === 'string' || typeof value === 'number') &&
    selectedFilterBuilderProp.valueType === filterBuilderValueTypes.BYTES
  ) {
    return formatBytes(value);
  }

  return String(value);
}

function getValue<T>(
  input: string | number | boolean,
  selectedFilterBuilderProp: FilterBuilderProp<T>
) {
  if (typeof input !== 'string') {
    return input;
  }

  if (selectedFilterBuilderProp.valueType === filterBuilderValueTypes.BYTES) {
    const match = input.match(/^(\d+)([kmgt](i?b)?)$/i);

    if (match && match.length > 1) {
      const [, value, unit] = input.match(/^(\d+)([kmgt](i?b)?)$/i) ?? [];

      switch (unit.toLowerCase()) {
        case 'k':
          return convertToBytes(value, 1, true);
        case 'm':
          return convertToBytes(value, 2, true);
        case 'g':
          return convertToBytes(value, 3, true);
        case 't':
          return convertToBytes(value, 4, true);
        case 'kb':
          return convertToBytes(value, 1, true);
        case 'mb':
          return convertToBytes(value, 2, true);
        case 'gb':
          return convertToBytes(value, 3, true);
        case 'tb':
          return convertToBytes(value, 4, true);
        case 'kib':
          return convertToBytes(value, 1, true);
        case 'mib':
          return convertToBytes(value, 2, true);
        case 'gib':
          return convertToBytes(value, 3, true);
        case 'tib':
          return convertToBytes(value, 4, true);
        default:
          return parseInt(value);
      }
    }
  }

  if (selectedFilterBuilderProp.type === filterBuilderTypes.NUMBER) {
    return parseInt(input as string);
  }

  return input;
}

interface FreeFormValue {
  name: string;
}

export interface FilterBuilderTag<V extends string | number | boolean>
  extends TagBase {
  id: V;
  name: string | number;
}

export interface FilterBuilderRowValueProps<
  T,
  V extends string | number | boolean
> {
  filterType?: string;
  filterValue: V[];
  sectionItems: T[];
  selectedFilterBuilderProp: FilterBuilderProp<T>;
  tagList: FilterBuilderTag<V>[];
  onChange: InputOnChange<V[]>;
}

function FilterBuilderRowValue<T, V extends string | number | boolean>({
  filterValue = [],
  selectedFilterBuilderProp,
  tagList,
  onChange,
}: FilterBuilderRowValueProps<T, V>) {
  const hasItems = !!tagList.length;

  const tags = useMemo(() => {
    return filterValue.map((id) => {
      if (hasItems) {
        const tag = tagList.find((t) => t.id === id);

        return {
          id,
          name: tag ? tag.name : '',
        };
      }

      return {
        id,
        name: getTagDisplayValue(id, selectedFilterBuilderProp),
      };
    });
  }, [filterValue, tagList, selectedFilterBuilderProp, hasItems]);

  const handleTagAdd = useCallback(
    (tag: FilterBuilderTag<V> | FreeFormValue) => {
      if ('id' in tag) {
        onChange({
          name: NAME,
          value: [...filterValue, tag.id],
        });

        return;
      }

      // Cast to V to avoid TS error combining V and value of V
      const value = getValue(tag.name, selectedFilterBuilderProp) as V;

      onChange({
        name: NAME,
        value: [...filterValue, value],
      });
    },
    [filterValue, selectedFilterBuilderProp, onChange]
  );

  const handleTagDelete = useCallback(
    ({ index }: { index: number }) => {
      const value = filterValue.filter((_v, i) => i !== index);

      onChange({
        name: NAME,
        value,
      });
    },
    [filterValue, onChange]
  );

  return (
    <TagInput
      name={NAME}
      tags={tags}
      tagList={tagList}
      allowNew={!tagList.length}
      kind={kinds.DEFAULT}
      delimiters={['Tab', 'Enter']}
      minQueryLength={0}
      tagComponent={FilterBuilderRowValueTag}
      onTagAdd={handleTagAdd}
      onTagDelete={handleTagDelete}
    />
  );
}

export default FilterBuilderRowValue;
