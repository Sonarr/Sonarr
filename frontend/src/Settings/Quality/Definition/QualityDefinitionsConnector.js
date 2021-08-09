import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchQualityDefinitions, saveQualityDefinitions } from 'Store/Actions/settingsActions';
import QualityDefinitions from './QualityDefinitions';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.qualityDefinitions,
    (state) => state.settings.advancedSettings,
    (qualityDefinitions, advancedSettings) => {
      const items = qualityDefinitions.items.map((item) => {
        const pendingChanges = qualityDefinitions.pendingChanges[item.id] || {};

        return Object.assign({}, item, pendingChanges);
      });

      return {
        ...qualityDefinitions,
        items,
        hasPendingChanges: !_.isEmpty(qualityDefinitions.pendingChanges),
        advancedSettings
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchFetchQualityDefinitions: fetchQualityDefinitions,
  dispatchSaveQualityDefinitions: saveQualityDefinitions
};

class QualityDefinitionsConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      dispatchFetchQualityDefinitions,
      dispatchSaveQualityDefinitions,
      onChildMounted
    } = this.props;

    dispatchFetchQualityDefinitions();
    onChildMounted(dispatchSaveQualityDefinitions);
  }

  componentDidUpdate(prevProps) {
    const {
      hasPendingChanges,
      isSaving,
      onChildStateChange
    } = this.props;

    if (
      prevProps.isSaving !== isSaving ||
      prevProps.hasPendingChanges !== hasPendingChanges
    ) {
      onChildStateChange({
        isSaving,
        hasPendingChanges
      });
    }
  }

  //
  // Render

  render() {
    return (
      <QualityDefinitions
        {...this.props}
      />
    );
  }
}

QualityDefinitionsConnector.propTypes = {
  isSaving: PropTypes.bool.isRequired,
  hasPendingChanges: PropTypes.bool.isRequired,
  dispatchFetchQualityDefinitions: PropTypes.func.isRequired,
  dispatchSaveQualityDefinitions: PropTypes.func.isRequired,
  onChildMounted: PropTypes.func.isRequired,
  onChildStateChange: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps, null)(QualityDefinitionsConnector);
