import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createTagDetailsSelector from 'Store/Selectors/createTagDetailsSelector';
import { deleteTag } from 'Store/Actions/tagActions';
import Tag from './Tag';

function createMapStateToProps() {
  return createSelector(
    createTagDetailsSelector(),
    (tagDetails) => {
      return {
        ...tagDetails
      };
    }
  );
}

const mapStateToProps = {
  onConfirmDeleteTag: deleteTag
};

export default connect(createMapStateToProps, mapStateToProps)(Tag);
