import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchDownloadClientSchema, selectDownloadClientSchema } from 'Store/Actions/settingsActions';
import AddDownloadClientModalContent from './AddDownloadClientModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.downloadClients,
    (downloadClients) => {
      const {
        isFetching,
        error,
        isPopulated,
        schema
      } = downloadClients;

      const usenetDownloadClients = _.filter(schema, { protocol: 'usenet' });
      const torrentDownloadClients = _.filter(schema, { protocol: 'torrent' });

      return {
        isFetching,
        error,
        isPopulated,
        usenetDownloadClients,
        torrentDownloadClients
      };
    }
  );
}

const mapDispatchToProps = {
  fetchDownloadClientSchema,
  selectDownloadClientSchema
};

class AddDownloadClientModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchDownloadClientSchema();
  }

  //
  // Listeners

  onDownloadClientSelect = ({ implementation }) => {
    this.props.selectDownloadClientSchema({ implementation });
    this.props.onModalClose({ downloadClientSelected: true });
  }

  //
  // Render

  render() {
    return (
      <AddDownloadClientModalContent
        {...this.props}
        onDownloadClientSelect={this.onDownloadClientSelect}
      />
    );
  }
}

AddDownloadClientModalContentConnector.propTypes = {
  fetchDownloadClientSchema: PropTypes.func.isRequired,
  selectDownloadClientSchema: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(AddDownloadClientModalContentConnector);
