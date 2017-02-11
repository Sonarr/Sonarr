import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import AddNotificationItem from './AddNotificationItem';
import styles from './AddNotificationModalContent.css';

class AddNotificationModalContent extends Component {

  //
  // Render

  render() {
    const {
      isFetching,
      error,
      isPopulated,
      schema,
      onNotificationSelect,
      onModalClose
    } = this.props;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          Add Notification
        </ModalHeader>

        <ModalBody>
          {
            isFetching &&
              <LoadingIndicator />
          }

          {
            !isFetching && !!error &&
              <div>Unable to add a new notification, please try again.</div>
          }

          {
            isPopulated && !error &&
              <div>
                <div className={styles.notifications}>
                  {
                    schema.map((notification) => {
                      return (
                        <AddNotificationItem
                          key={notification.implementation}
                          implementation={notification.implementation}
                          {...notification}
                          onNotificationSelect={onNotificationSelect}
                        />
                      );
                    })
                  }
                </div>
              </div>
          }
        </ModalBody>
        <ModalFooter>
          <Button
            onPress={onModalClose}
          >
            Close
          </Button>
        </ModalFooter>
      </ModalContent>
    );
  }
}

AddNotificationModalContent.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  isPopulated: PropTypes.bool.isRequired,
  schema: PropTypes.arrayOf(PropTypes.object).isRequired,
  onNotificationSelect: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default AddNotificationModalContent;
