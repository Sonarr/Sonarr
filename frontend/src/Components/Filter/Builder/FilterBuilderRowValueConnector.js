import _ from 'lodash';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import sortByName from 'Utilities/Array/sortByName';
import { filterBuilderTypes } from 'Helpers/Props';
import FilterBuilderRowValue from './FilterBuilderRowValue';

function createTagListSelector() {
  return createSelector(
    (state, { sectionItems }) => sectionItems,
    (state, { selectedFilterBuilderProp }) => selectedFilterBuilderProp,
    (sectionItems, selectedFilterBuilderProp) => {
      if (
        selectedFilterBuilderProp.type === filterBuilderTypes.NUMBER ||
        selectedFilterBuilderProp.type === filterBuilderTypes.STRING
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
