import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { saveRemotePathMapping, setRemotePathMappingValue } from 'Store/Actions/settingsActions';
import selectSettings from 'Store/Selectors/selectSettings';
import EditRemotePathMappingModalContent from './EditRemotePathMappingModalContent';

const newRemotePathMapping = {
  host: '',
  remotePath: '',
  localPath: ''
};

const selectDownloadClientHosts = createSelector(
  (state) => state.settings.downloadClients.items,
  (downloadClients) => {
    const hosts = downloadClients.reduce((acc, downloadClient) => {
      const name = downloadClient.name;
      const host = downloadClient.fields.find((field) => {
        return field.name === 'host';
      });

      if (host) {
        const group = acc[host.value] = acc[host.value] || [];
        group.push(name);
      }

      return acc;
    }, {});

    return Object.keys(hosts).map((host) => {
      return {
        key: host,
        value: host,
        hint: `${hosts[host].join(', ')}`
      };
    });
  }
);

function createRemotePathMappingSelector() {
  return createSelector(
    (state, { id }) => id,
    (state) => state.settings.remotePathMappings,
    selectDownloadClientHosts,
    (id, remotePathMappings, downloadClientHosts) => {
      const {
        isFetching,
        error,
        isSaving,
        saveError,
        pendingChanges,
        items
      } = remotePathMappings;

      const mapping = id ? _.find(items, { id }) : newRemotePathMapping;
      const settings = selectSettings(mapping, pendingChanges, saveError);

      return {
        id,
        isFetching,
        error,
        isSaving,
        saveError,
        item: settings.settings,
        ...settings,
        downloadClientHosts
      };
    }
  );
}

function createMapStateToProps() {
  return createSelector(
    createRemotePathMappingSelector(),
    (remotePathMapping) => {
      return {
        ...remotePathMapping
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchSetRemotePathMappingValue: setRemotePathMappingValue,
  dispatchSaveRemotePathMapping: saveRemotePathMapping
};

class EditRemotePathMappingModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    if (!this.props.id) {
      Object.keys(newRemotePathMapping).forEach((name) => {
        this.props.dispatchSetRemotePathMappingValue({
          name,
          value: newRemotePathMapping[name]
        });
      });
    }
  }

  componentDidUpdate(prevProps, prevState) {
    if (prevProps.isSaving && !this.props.isSaving && !this.props.saveError) {
      this.props.onModalClose();
    }
  }

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.dispatchSetRemotePathMappingValue({ name, value });
  };

  onSavePress = () => {
    this.props.dispatchSaveRemotePathMapping({ id: this.props.id });
  };

  //
  // Render

  render() {
    return (
      <EditRemotePathMappingModalContent
        {...this.props}
        onSavePress={this.onSavePress}
        onInputChange={this.onInputChange}
      />
    );
  }
}

EditRemotePathMappingModalContentConnector.propTypes = {
  id: PropTypes.number,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  item: PropTypes.object.isRequired,
  dispatchSetRemotePathMappingValue: PropTypes.func.isRequired,
  dispatchSaveRemotePathMapping: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(EditRemotePathMappingModalContentConnector);
