import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchDownloadClients } from 'Store/Actions/settingsActions';
import sortByName from 'Utilities/Array/sortByName';
import EnhancedSelectInput from './EnhancedSelectInput';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.downloadClients,
    (state, { includeAny }) => includeAny,
    (state, { protocol }) => protocol,
    (downloadClients, includeAny, protocolFilter) => {
      const {
        isFetching,
        isPopulated,
        error,
        items
      } = downloadClients;

      const filteredItems = items.filter((item) => item.protocol === protocolFilter);

      const values = _.map(filteredItems.sort(sortByName), (downloadClient) => {
        return {
          key: downloadClient.id,
          value: downloadClient.name
        };
      });

      if (includeAny) {
        values.unshift({
          key: 0,
          value: '(Any)'
        });
      }

      return {
        isFetching,
        isPopulated,
        error,
        values
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchFetchDownloadClients: fetchDownloadClients
};

class DownloadClientSelectInputConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    if (!this.props.isPopulated) {
      this.props.dispatchFetchDownloadClients();
    }
  }

  //
  // Listeners

  onChange = ({ name, value }) => {
    this.props.onChange({ name, value: parseInt(value) });
  };

  //
  // Render

  render() {
    return (
      <EnhancedSelectInput
        {...this.props}
        onChange={this.onChange}
      />
    );
  }
}

DownloadClientSelectInputConnector.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  name: PropTypes.string.isRequired,
  value: PropTypes.oneOfType([PropTypes.number, PropTypes.string]).isRequired,
  values: PropTypes.arrayOf(PropTypes.object).isRequired,
  includeAny: PropTypes.bool.isRequired,
  onChange: PropTypes.func.isRequired,
  dispatchFetchDownloadClients: PropTypes.func.isRequired
};

DownloadClientSelectInputConnector.defaultProps = {
  includeAny: false,
  protocol: 'torrent'
};

export default connect(createMapStateToProps, mapDispatchToProps)(DownloadClientSelectInputConnector);
