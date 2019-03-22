import { connect } from 'react-redux';
import { deleteRootFolder } from 'Store/Actions/rootFolderActions';
import RootFolderRow from './RootFolderRow';

function createMapDispatchToProps(dispatch, props) {
  return {
    onDeletePress() {
      dispatch(deleteRootFolder({ id: props.id }));
    }
  };
}

export default connect(null, createMapDispatchToProps)(RootFolderRow);
