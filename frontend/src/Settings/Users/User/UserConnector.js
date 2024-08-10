import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { deleteUser } from 'Store/Actions/usersActions';
import createUserDetailsSelector from 'Store/Selectors/createUserDetailsSelector';
import User from './User';

function createMapStateToProps() {
  return createSelector(
    createUserDetailsSelector(),
    (userDetails) => {
      return {
        ...userDetails
      };
    }
  );
}

const mapDispatchToProps = {
  onConfirmDeleteUser: deleteUser
};

export default connect(createMapStateToProps, mapDispatchToProps)(User);
