import PropTypes from 'prop-types';
import React from 'react';
import { kinds } from 'Helpers/Props';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Button from 'Components/Link/Button';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import UpdateChanges from 'System/Updates/UpdateChanges';
import styles from './AppUpdatedModalContent.css';

function AppUpdatedModalContent(props) {
  const {
    version,
    isPopulated,
    error,
    items,
    onSeeChangesPress,
    onModalClose
  } = props;

  const update = items[0];

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        Sonarr Updated
      </ModalHeader>

      <ModalBody>
        <div>
          Version <span className={styles.version}>{version}</span> of Sonarr has been installed, in order to get the latest changes you'll need to reload Sonarr.
        </div>

        {
          isPopulated && !error && !!update &&
            <div>
              {
                !update.changes &&
                  <div className={styles.maintenance}>Maintenance release</div>
              }

              {
                !!update.changes &&
                  <div>
                    <div className={styles.changes}>
                      What's new?
                    </div>

                    <UpdateChanges
                      title="New"
                      changes={update.changes.new}
                    />

                    <UpdateChanges
                      title="Fixed"
                      changes={update.changes.fixed}
                    />
                  </div>
              }
            </div>
        }

        {
          !isPopulated && !error &&
            <LoadingIndicator />
        }
      </ModalBody>

      <ModalFooter>
        <Button
          onPress={onSeeChangesPress}
        >
          Recent Changes
        </Button>

        <Button
          kind={kinds.PRIMARY}
          onPress={onModalClose}
        >
          Reload
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

AppUpdatedModalContent.propTypes = {
  version: PropTypes.string.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  onSeeChangesPress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default AppUpdatedModalContent;
