import PropTypes from 'prop-types';
import React from 'react';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { kinds } from 'Helpers/Props';
import UpdateChanges from 'System/Updates/UpdateChanges';
import styles from './AppUpdatedModalContent.css';

function mergeUpdates(items, version, prevVersion) {
  let installedIndex = items.findIndex((u) => u.version === version);
  let installedPreviouslyIndex = items.findIndex((u) => u.version === prevVersion);

  if (installedIndex === -1) {
    installedIndex = 0;
  }

  if (installedPreviouslyIndex === -1) {
    installedPreviouslyIndex = items.length;
  } else if (installedPreviouslyIndex === installedIndex && items.length) {
    installedPreviouslyIndex += 1;
  }

  const appliedUpdates = items.slice(installedIndex, installedPreviouslyIndex);

  if (!appliedUpdates.length) {
    return null;
  }

  const appliedChanges = { new: [], fixed: [] };
  appliedUpdates.forEach((u) => {
    if (u.changes) {
      appliedChanges.new.push(... u.changes.new);
      appliedChanges.fixed.push(... u.changes.fixed);
    }
  });

  const mergedUpdate = Object.assign({}, appliedUpdates[0], { changes: appliedChanges });

  if (!appliedChanges.new.length && !appliedChanges.fixed.length) {
    mergedUpdate.changes = null;
  }

  return mergedUpdate;
}

function AppUpdatedModalContent(props) {
  const {
    version,
    prevVersion,
    isPopulated,
    error,
    items,
    onSeeChangesPress,
    onModalClose
  } = props;

  const update = mergeUpdates(items, version, prevVersion);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        Sonarr Updated
      </ModalHeader>

      <ModalBody>
        <div>
          Sonarr has been updated to version <span className={styles.version}>{version}</span>, in order to get the latest changes you'll need to reload Sonarr.
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
  prevVersion: PropTypes.string,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  onSeeChangesPress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default AppUpdatedModalContent;
