import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { deleteNotification, fetchNotifications } from 'Store/Actions/settingsActions';
import createSortedSectionSelector from 'Store/Selectors/createSortedSectionSelector';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import sortByName from 'Utilities/Array/sortByName';
import Notifications from './Notifications';

function createMapStateToProps() {
  return createSelector(
    createSortedSectionSelector('settings.notifications', sortByName),
    createTagsSelector(),
    (notifications, tagList) => {
      return {
        ...notifications,
        tagList
      };
    }
  );
}

const mapDispatchToProps = {
  fetchNotifications,
  deleteNotification
};

class NotificationsConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchNotifications();
  }

  //
  // Listeners

  onConfirmDeleteNotification = (id) => {
    this.props.deleteNotification({ id });
  };

  //
  // Render

  render() {
    return (
      <Notifications
        {...this.props}
        onConfirmDeleteNotification={this.onConfirmDeleteNotification}
      />
    );
  }
}

NotificationsConnector.propTypes = {
  fetchNotifications: PropTypes.func.isRequired,
  deleteNotification: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(NotificationsConnector);
