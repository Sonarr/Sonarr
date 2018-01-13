import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import TagList from './TagList';

function createMapStateToProps() {
  return createSelector(
    createTagsSelector(),
    (tagList) => {
      return {
        tagList
      };
    }
  );
}

export default connect(createMapStateToProps)(TagList);
