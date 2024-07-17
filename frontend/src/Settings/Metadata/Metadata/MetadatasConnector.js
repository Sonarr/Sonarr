import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchMetadata } from 'Store/Actions/settingsActions';
import createSortedSectionSelector from 'Store/Selectors/createSortedSectionSelector';
import sortByProp from 'Utilities/Array/sortByProp';
import Metadatas from './Metadatas';

function createMapStateToProps() {
  return createSelector(
    createSortedSectionSelector('settings.metadata', sortByProp('name')),
    (metadata) => metadata
  );
}

const mapDispatchToProps = {
  fetchMetadata
};

class MetadatasConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchMetadata();
  }

  //
  // Render

  render() {
    return (
      <Metadatas
        {...this.props}
        onConfirmDeleteMetadata={this.onConfirmDeleteMetadata}
      />
    );
  }
}

MetadatasConnector.propTypes = {
  fetchMetadata: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(MetadatasConnector);
