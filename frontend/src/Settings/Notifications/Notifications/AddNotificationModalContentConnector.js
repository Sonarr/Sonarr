import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchNotificationSchema, selectNotificationSchema } from 'Store/Actions/settingsActions';
import AddNotificationModalContent from './AddNotificationModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.notifications,
    (notifications) => {
      const {
        isSchemaFetching,
        isSchemaPopulated,
        schemaError,
        schema
      } = notifications;

      return {
        isSchemaFetching,
        isSchemaPopulated,
        schemaError,
        schema
      };
    }
  );
}

const mapDispatchToProps = {
  fetchNotificationSchema,
  selectNotificationSchema
};

class AddNotificationModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchNotificationSchema();
  }

  //
  // Listeners

  onNotificationSelect = ({ implementation, name }) => {
    this.props.selectNotificationSchema({ implementation, presetName: name });
    this.props.onModalClose({ notificationSelected: true });
  };

  //
  // Render

  render() {
    return (
      <AddNotificationModalContent
        {...this.props}
        onNotificationSelect={this.onNotificationSelect}
      />
    );
  }
}

AddNotificationModalContentConnector.propTypes = {
  fetchNotificationSchema: PropTypes.func.isRequired,
  selectNotificationSchema: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(AddNotificationModalContentConnector);
