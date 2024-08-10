import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchUsers } from 'Store/Actions/settingsActions';
import Users from './Users';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.users,
    (users) => {

      const isFetching = users.isFetching;
      const error = users.error;
      const isPopulated = users.isPopulated;

      return {
        ...users,
        isFetching,
        error,
        isPopulated
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchFetchUsers: fetchUsers
};

class UsersConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      dispatchFetchUsers
    } = this.props;
    dispatchFetchUsers();
  }

  //
  // Render

  render() {
    return (
      <Users
        {...this.props}
      />
    );
  }
}

UsersConnector.propTypes = {
  dispatchFetchUsers: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(UsersConnector);
