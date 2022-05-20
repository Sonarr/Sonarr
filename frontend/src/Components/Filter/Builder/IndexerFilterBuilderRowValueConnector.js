import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import tagShape from 'Helpers/Props/Shapes/tagShape';
import { fetchIndexers } from 'Store/Actions/settingsActions';
import FilterBuilderRowValue from './FilterBuilderRowValue';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.indexers,
    (qualityProfiles) => {
      const {
        isFetching,
        isPopulated,
        error,
        items
      } = qualityProfiles;

      const tagList = items.map((item) => {
        return {
          id: item.id,
          name: item.name
        };
      });

      return {
        isFetching,
        isPopulated,
        error,
        tagList
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchFetchIndexers: fetchIndexers
};

class IndexerFilterBuilderRowValueConnector extends Component {

  //
  // Lifecycle

  componentDidMount = () => {
    if (!this.props.isPopulated) {
      this.props.dispatchFetchIndexers();
    }
  };

  //
  // Render

  render() {
    const {
      isFetching,
      isPopulated,
      error,
      ...otherProps
    } = this.props;

    return (
      <FilterBuilderRowValue
        {...otherProps}
      />
    );
  }
}

IndexerFilterBuilderRowValueConnector.propTypes = {
  tagList: PropTypes.arrayOf(PropTypes.shape(tagShape)).isRequired,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  dispatchFetchIndexers: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(IndexerFilterBuilderRowValueConnector);
