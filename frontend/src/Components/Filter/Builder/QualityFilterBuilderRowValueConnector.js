import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import tagShape from 'Helpers/Props/Shapes/tagShape';
import { fetchQualityProfileSchema } from 'Store/Actions/settingsActions';
import getQualities from 'Utilities/Quality/getQualities';
import FilterBuilderRowValue from './FilterBuilderRowValue';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.qualityProfiles,
    (qualityProfiles) => {
      const {
        isSchemaFetching: isFetching,
        isSchemaPopulated: isPopulated,
        schemaError: error,
        schema
      } = qualityProfiles;

      const tagList = getQualities(schema.items);

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
  dispatchFetchQualityProfileSchema: fetchQualityProfileSchema
};

class QualityFilterBuilderRowValueConnector extends Component {

  //
  // Lifecycle

  componentDidMount = () => {
    if (!this.props.isPopulated) {
      this.props.dispatchFetchQualityProfileSchema();
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

QualityFilterBuilderRowValueConnector.propTypes = {
  tagList: PropTypes.arrayOf(PropTypes.shape(tagShape)).isRequired,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  dispatchFetchQualityProfileSchema: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(QualityFilterBuilderRowValueConnector);
