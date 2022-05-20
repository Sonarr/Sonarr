import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { cloneIndexer, deleteIndexer, fetchIndexers } from 'Store/Actions/settingsActions';
import createSortedSectionSelector from 'Store/Selectors/createSortedSectionSelector';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import sortByName from 'Utilities/Array/sortByName';
import Indexers from './Indexers';

function createMapStateToProps() {
  return createSelector(
    createSortedSectionSelector('settings.indexers', sortByName),
    createTagsSelector(),
    (indexers, tagList) => {
      return {
        ...indexers,
        tagList
      };
    }
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
  };

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
