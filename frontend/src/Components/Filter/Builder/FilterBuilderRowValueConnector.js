import _ from 'lodash';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { filterBuilderTypes } from 'Helpers/Props';
import FilterBuilderRowValue from './FilterBuilderRowValue';

function createTagListSelector() {
  return createSelector(
    (state, { sectionItems }) => _.get(state, sectionItems),
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
        items = sectionItems.map(selectedFilterBuilderProp.optionsSelector);
      } else {
        items = sectionItems.map((item) => {
          const name = item[selectedFilterBuilderProp.name];

          return {
            id: name,
            name
          };
        });
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
