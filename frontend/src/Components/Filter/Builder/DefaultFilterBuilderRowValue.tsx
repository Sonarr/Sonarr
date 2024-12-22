import React, { useMemo } from 'react';
import { FilterBuilderPropOption } from 'App/State/AppState';
import { filterBuilderTypes } from 'Helpers/Props';
import * as filterTypes from 'Helpers/Props/filterTypes';
import sortByProp from 'Utilities/Array/sortByProp';
import FilterBuilderRowValue, {
  FilterBuilderRowValueProps,
} from './FilterBuilderRowValue';

type DefaultFilterBuilderRowValueProps<T> = Omit<
  FilterBuilderRowValueProps<T, string>,
  'tagList'
>;

function DefaultFilterBuilderRowValue<T>({
  filterType,
  sectionItems,
  selectedFilterBuilderProp,
  ...otherProps
}: DefaultFilterBuilderRowValueProps<T>) {
  const tagList = useMemo(() => {
    if (
      ((selectedFilterBuilderProp.type === filterBuilderTypes.NUMBER ||
        selectedFilterBuilderProp.type === filterBuilderTypes.STRING) &&
        filterType !== filterTypes.EQUAL &&
        filterType !== filterTypes.NOT_EQUAL) ||
      !selectedFilterBuilderProp.optionsSelector
    ) {
      return [];
    }

    let items: FilterBuilderPropOption[] = [];

    if (selectedFilterBuilderProp.optionsSelector) {
      items = selectedFilterBuilderProp
        .optionsSelector(sectionItems)
        .filter(
          (value, index, array) =>
            array.findIndex((v) => v.id === value.id) === index
        );
    } else {
      items = sectionItems
        .reduce<FilterBuilderPropOption[]>((acc, item) => {
          // @ts-expect-error - can't guarantee that the name property exists on the item
          const name = item[selectedFilterBuilderProp.name];

          // DOn't add invalid values or items that already exist
          if (name && acc.findIndex((a) => a.id === name) === -1) {
            acc.push({
              id: name,
              name,
            });
          }

          return acc;
        }, [])
        .sort(sortByProp('name'));
    }

    return items;
  }, [filterType, sectionItems, selectedFilterBuilderProp]);

  return (
    <FilterBuilderRowValue
      {...otherProps}
      filterType={filterType}
      sectionItems={sectionItems}
      selectedFilterBuilderProp={selectedFilterBuilderProp}
      tagList={tagList}
    />
  );
}

export default DefaultFilterBuilderRowValue;
