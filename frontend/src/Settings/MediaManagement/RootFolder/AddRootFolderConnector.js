import { connect } from 'react-redux';
import AddRootFolder from './AddRootFolder';
import { addRootFolder } from 'Store/Actions/rootFolderActions';

function createMapDispatchToProps(dispatch) {
  return {
    onNewRootFolderSelect(path) {
      dispatch(addRootFolder({ path }));
    }
  };
}

export default connect(null, createMapDispatchToProps)(AddRootFolder);
