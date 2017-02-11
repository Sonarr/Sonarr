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
    (qualityDefinitions) => {
      const items = qualityDefinitions.items.map((item) => {
        const pendingChanges = qualityDefinitions.pendingChanges[item.id] || {};

        return Object.assign({}, item, pendingChanges);
      });

      return {
        ...qualityDefinitions,
        items,
        hasPendingChanges: !_.isEmpty(qualityDefinitions.pendingChanges)
      };
    }
  );
}

const mapDispatchToProps = {
  fetchQualityDefinitions,
  saveQualityDefinitions
};

class QualityDefinitionsConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchQualityDefinitions();
  }

  componentDidUpdate(prevProps) {
    const {
      hasPendingChanges
    } = this.props;

    if (hasPendingChanges !== prevProps.hasPendingChanges) {
      this.props.onHasPendingChange(hasPendingChanges);
    }
  }

  //
  // Control

  save = () => {
    this.props.saveQualityDefinitions();
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
  hasPendingChanges: PropTypes.bool.isRequired,
  fetchQualityDefinitions: PropTypes.func.isRequired,
  saveQualityDefinitions: PropTypes.func.isRequired,
  onHasPendingChange: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps, null, { withRef: true })(QualityDefinitionsConnector);
