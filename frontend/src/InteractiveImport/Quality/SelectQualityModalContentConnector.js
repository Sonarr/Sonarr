import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import getQualities from 'Utilities/Quality/getQualities';
import { fetchQualityProfileSchema } from 'Store/Actions/settingsActions';
import { updateInteractiveImportItem } from 'Store/Actions/interactiveImportActions';
import SelectQualityModalContent from './SelectQualityModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.qualityProfiles,
    (qualityProfiles) => {
      const {
        isFetchingSchema: isFetching,
        isSchemaPopulated: isPopulated,
        schemaError: error,
        schema
      } = qualityProfiles;

      return {
        isFetching,
        isPopulated,
        error,
        items: getQualities(schema.items)
      };
    }
  );
}

const mapDispatchToProps = {
  fetchQualityProfileSchema,
  updateInteractiveImportItem
};

class SelectQualityModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount = () => {
    if (!this.props.isPopulated) {
      this.props.fetchQualityProfileSchema();
    }
  }

  //
  // Listeners

  onQualitySelect = ({ qualityId, proper, real }) => {
    const quality = _.find(this.props.items,
      (item) => item.id === qualityId);

    const revision = {
      version: proper ? 2 : 1,
      real: real ? 1 : 0
    };

    this.props.updateInteractiveImportItem({
      id: this.props.id,
      quality: {
        quality,
        revision
      }
    });

    this.props.onModalClose(true);
  }

  //
  // Render

  render() {
    return (
      <SelectQualityModalContent
        {...this.props}
        onQualitySelect={this.onQualitySelect}
      />
    );
  }
}

SelectQualityModalContentConnector.propTypes = {
  id: PropTypes.number.isRequired,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  fetchQualityProfileSchema: PropTypes.func.isRequired,
  updateInteractiveImportItem: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(SelectQualityModalContentConnector);
