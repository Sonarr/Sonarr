import { connect } from 'react-redux';
import { addRootFolder } from 'Store/Actions/rootFolderActions';
import AddRootFolder from './AddRootFolder';

function createMapDispatchToProps(dispatch) {
  return {
    onNewRootFolderSelect(path) {
      dispatch(addRootFolder({ path }));
    }
  };
}

export default connect(null, createMapDispatchToProps)(AddRootFolder);
