import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import sortByName from 'Utilities/Array/sortByName';
import SelectInput from './SelectInput';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.indexers,
    (state, { includeNoChange }) => includeNoChange,
    (state, { includeMixed }) => includeMixed,
    (indexers, includeNoChange, includeMixed) => {
      const values = _.map(indexers.items.sort(sortByName), (indexer) => {
        return {
          key: indexer.id,
          value: indexer.name
        };
      });

      if (includeNoChange) {
        values.unshift({
          key: 'noChange',
          value: 'No Change',
          disabled: true
        });
      }

      if (includeMixed) {
        values.unshift({
          key: 'mixed',
          value: '(Mixed)',
          disabled: true
        });
      }

      return {
        values
      };
    }
  );
}

class IndexerSelectInputConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      name,
      value,
      values
    } = this.props;

    if (!value || !_.some(values, (option) => parseInt(option.key) === value)) {
      const firstValue = _.find(values, (option) => !isNaN(parseInt(option.key)));

      if (firstValue) {
        this.onChange({ name, value: firstValue.key });
      }
    }
  }

  //
  // Listeners

  onChange = ({ name, value }) => {
    this.props.onChange({ name, value: parseInt(value) });
  }

  //
  // Render

  render() {
    return (
      <SelectInput
        {...this.props}
        onChange={this.onChange}
      />
    );
  }
}

IndexerSelectInputConnector.propTypes = {
  name: PropTypes.string.isRequired,
  value: PropTypes.oneOfType([PropTypes.number, PropTypes.string]),
  values: PropTypes.arrayOf(PropTypes.object).isRequired,
  includeNoChange: PropTypes.bool.isRequired,
  onChange: PropTypes.func.isRequired
};

IndexerSelectInputConnector.defaultProps = {
  includeNoChange: false
};

export default connect(createMapStateToProps)(IndexerSelectInputConnector);
