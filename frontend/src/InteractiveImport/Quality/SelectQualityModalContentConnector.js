import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import getQualities from 'Utilities/Quality/getQualities';
import { fetchQualityProfileSchema } from 'Store/Actions/settingsActions';
import { updateInteractiveImportItems, reprocessInteractiveImportItems } from 'Store/Actions/interactiveImportActions';
import SelectQualityModalContent from './SelectQualityModalContent';

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
  dispatchFetchQualityProfileSchema: fetchQualityProfileSchema,
  dispatchUpdateInteractiveImportItems: updateInteractiveImportItems,
  dispatchReprocessInteractiveImportItems: reprocessInteractiveImportItems
};

class SelectQualityModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount = () => {
    if (!this.props.isPopulated) {
      this.props.dispatchFetchQualityProfileSchema();
    }
  }

  //
  // Listeners

  onQualitySelect = ({ qualityId, proper, real }) => {
    const {
      ids,
      dispatchUpdateInteractiveImportItems,
      dispatchReprocessInteractiveImportItems
    } = this.props;

    const quality = _.find(this.props.items,
      (item) => item.id === qualityId);

    const revision = {
      version: proper ? 2 : 1,
      real: real ? 1 : 0
    };

    dispatchUpdateInteractiveImportItems({
      ids,
      quality: {
        quality,
        revision
      }
    });

    dispatchReprocessInteractiveImportItems({ ids });

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
  ids: PropTypes.arrayOf(PropTypes.number).isRequired,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  dispatchFetchQualityProfileSchema: PropTypes.func.isRequired,
  dispatchUpdateInteractiveImportItems: PropTypes.func.isRequired,
  dispatchReprocessInteractiveImportItems: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(SelectQualityModalContentConnector);
