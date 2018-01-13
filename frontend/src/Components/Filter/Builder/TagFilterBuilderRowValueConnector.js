import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import FilterBuilderRowValue from './FilterBuilderRowValue';

function createMapStateToProps() {
  return createSelector(
    createTagsSelector(),
    (tagList) => {
      return {
        tagList: tagList.map((tag) => {
          const {
            id,
            label: name
          } = tag;

          return {
            id,
            name
          };
        })
      };
    }
  );
}

export default connect(createMapStateToProps)(FilterBuilderRowValue);
