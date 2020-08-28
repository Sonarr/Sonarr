import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import sortByName from 'Utilities/Array/sortByName';
import createSortedSectionSelector from 'Store/Selectors/createSortedSectionSelector';
import { fetchIndexers, deleteIndexer, cloneIndexer } from 'Store/Actions/settingsActions';
import Indexers from './Indexers';

function createMapStateToProps() {
  return createSelector(
    createSortedSectionSelector('settings.indexers', sortByName),
    (indexers) => indexers
  );
}

const mapDispatchToProps = {
  dispatchFetchIndexers: fetchIndexers,
  dispatchDeleteIndexer: deleteIndexer,
  dispatchCloneIndexer: cloneIndexer
};

class IndexersConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.dispatchFetchIndexers();
  }

  //
  // Listeners

  onConfirmDeleteIndexer = (id) => {
    this.props.dispatchDeleteIndexer({ id });
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
  dispatchFetchIndexers: PropTypes.func.isRequired,
  dispatchDeleteIndexer: PropTypes.func.isRequired,
  dispatchCloneIndexer: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(IndexersConnector);
