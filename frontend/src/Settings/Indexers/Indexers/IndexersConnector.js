import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchIndexers, deleteIndexer } from 'Store/Actions/settingsActions';
import Indexers from './Indexers';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.indexers,
    (indexers) => {
      return {
        ...indexers
      };
    }
  );
}

const mapDispatchToProps = {
  fetchIndexers,
  deleteIndexer
};

class IndexersConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchIndexers();
  }

  //
  // Listeners

  onConfirmDeleteIndexer = (id) => {
    this.props.deleteIndexer({ id });
  }

  //
  // Render

  render() {
    return (
      <Indexers
        {...this.props}
        onConfirmDeleteIndexer={this.onConfirmDeleteIndexer}
      />
    );
  }
}

IndexersConnector.propTypes = {
  fetchIndexers: PropTypes.func.isRequired,
  deleteIndexer: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(IndexersConnector);
