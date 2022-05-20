import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchIndexers } from 'Store/Actions/settingsActions';
import sortByName from 'Utilities/Array/sortByName';
import EnhancedSelectInput from './EnhancedSelectInput';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.indexers,
    (state, { includeAny }) => includeAny,
    (indexers, includeAny) => {
      const {
        isFetching,
        isPopulated,
        error,
        items
      } = indexers;

      const values = _.map(items.sort(sortByName), (indexer) => {
        return {
          key: indexer.id,
          value: indexer.name
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
  dispatchFetchIndexers: fetchIndexers
};

class IndexerSelectInputConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    if (!this.props.isPopulated) {
      this.props.dispatchFetchIndexers();
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

IndexerSelectInputConnector.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  name: PropTypes.string.isRequired,
  value: PropTypes.oneOfType([PropTypes.number, PropTypes.string]).isRequired,
  values: PropTypes.arrayOf(PropTypes.object).isRequired,
  includeAny: PropTypes.bool.isRequired,
  onChange: PropTypes.func.isRequired,
  dispatchFetchIndexers: PropTypes.func.isRequired
};

IndexerSelectInputConnector.defaultProps = {
  includeAny: false
};

export default connect(createMapStateToProps, mapDispatchToProps)(IndexerSelectInputConnector);
