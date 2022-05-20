import _ from 'lodash';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { filterBuilderTypes } from 'Helpers/Props';
import * as filterTypes from 'Helpers/Props/filterTypes';
import sortByName from 'Utilities/Array/sortByName';
import FilterBuilderRowValue from './FilterBuilderRowValue';

function createTagListSelector() {
  return createSelector(
    (state, { filterType }) => filterType,
    (state, { sectionItems }) => sectionItems,
    (state, { selectedFilterBuilderProp }) => selectedFilterBuilderProp,
    (filterType, sectionItems, selectedFilterBuilderProp) => {
      if (
        (selectedFilterBuilderProp.type === filterBuilderTypes.NUMBER ||
        selectedFilterBuilderProp.type === filterBuilderTypes.STRING) &&
        filterType !== filterTypes.EQUAL &&
        filterType !== filterTypes.NOT_EQUAL ||
        !selectedFilterBuilderProp.optionsSelector
      ) {
        return [];
      }

      let items = [];

      if (selectedFilterBuilderProp.optionsSelector) {
        items = selectedFilterBuilderProp.optionsSelector(sectionItems);
      } else {
        items = sectionItems.reduce((acc, item) => {
          const name = item[selectedFilterBuilderProp.name];

          if (name) {
            acc.push({
              id: name,
              name
            });
          }

          return acc;
        }, []).sort(sortByName);
      }

      return _.uniqBy(items, 'id');
    }
  );
}

function createMapStateToProps() {
  return createSelector(
    createTagListSelector(),
    (tagList) => {
      return {
        tagList
      };
    }
  );
}

export default connect(createMapStateToProps)(FilterBuilderRowValue);
