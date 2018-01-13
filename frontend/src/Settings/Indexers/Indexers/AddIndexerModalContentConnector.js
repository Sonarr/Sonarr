import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchIndexerSchema, selectIndexerSchema } from 'Store/Actions/settingsActions';
import AddIndexerModalContent from './AddIndexerModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.indexers,
    (indexers) => {
      const {
        isFetching,
        error,
        isPopulated,
        schema
      } = indexers;

      const usenetIndexers = _.filter(schema, { protocol: 'usenet' });
      const torrentIndexers = _.filter(schema, { protocol: 'torrent' });

      return {
        isFetching,
        error,
        isPopulated,
        usenetIndexers,
        torrentIndexers
      };
    }
  );
}

const mapDispatchToProps = {
  fetchIndexerSchema,
  selectIndexerSchema
};

class AddIndexerModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchIndexerSchema();
  }

  //
  // Listeners

  onIndexerSelect = ({ implementation, name }) => {
    this.props.selectIndexerSchema({ implementation, presetName: name });
    this.props.onModalClose({ indexerSelected: true });
  }

  //
  // Render

  render() {
    return (
      <AddIndexerModalContent
        {...this.props}
        onIndexerSelect={this.onIndexerSelect}
      />
    );
  }
}

AddIndexerModalContentConnector.propTypes = {
  fetchIndexerSchema: PropTypes.func.isRequired,
  selectIndexerSchema: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(AddIndexerModalContentConnector);
