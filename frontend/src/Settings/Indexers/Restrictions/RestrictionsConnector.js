import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchRestrictions, deleteRestriction } from 'Store/Actions/settingsActions';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import Restrictions from './Restrictions';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.restrictions,
    createTagsSelector(),
    (restrictions, tagList) => {
      return {
        ...restrictions,
        tagList
      };
    }
  );
}

const mapDispatchToProps = {
  fetchRestrictions,
  deleteRestriction
};

class RestrictionsConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchRestrictions();
  }

  //
  // Listeners

  onConfirmDeleteRestriction = (id) => {
    this.props.deleteRestriction({ id });
  }

  //
  // Render

  render() {
    return (
      <Restrictions
        {...this.props}
        onConfirmDeleteRestriction={this.onConfirmDeleteRestriction}
      />
    );
  }
}

RestrictionsConnector.propTypes = {
  fetchRestrictions: PropTypes.func.isRequired,
  deleteRestriction: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(RestrictionsConnector);
