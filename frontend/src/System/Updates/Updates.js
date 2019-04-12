import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component, Fragment } from 'react';
import { icons, kinds } from 'Helpers/Props';
import formatDate from 'Utilities/Date/formatDate';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import SpinnerButton from 'Components/Link/SpinnerButton';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import UpdateChanges from './UpdateChanges';
import styles from './Updates.css';

class Updates extends Component {

  //
  // Render

  render() {
    const {
      currentVersion,
      isFetching,
      isPopulated,
      updatesError,
      generalSettingsError,
      items,
      isInstallingUpdate,
      updateMechanism,
      shortDateFormat,
      onInstallLatestPress
    } = this.props;

    const hasError = !!(updatesError || generalSettingsError);
    const hasUpdates = isPopulated && !hasError && items.length > 0;
    const noUpdates = isPopulated && !hasError && !items.length;
    const hasUpdateToInstall = hasUpdates && _.some(items, { installable: true, latest: true });
    const noUpdateToInstall = hasUpdates && !hasUpdateToInstall;

    const externalUpdaterMessages = {
      external: 'Unable to update Sonarr directly, Sonarr is configured to use an external update mechanism',
      apt: 'Unable to update Sonarr directly, use apt to install the update',
      docker: 'Unable to update Sonarr directly, update the docker container to receive the update'
    };

    return (
      <PageContent title="Updates">
        <PageContentBodyConnector>
          {
            !isPopulated && !hasError &&
              <LoadingIndicator />
          }

          {
            noUpdates &&
              <div>No updates are available</div>
          }

          {
            hasUpdateToInstall &&
              <div className={styles.messageContainer}>
                {
                  updateMechanism === 'builtIn' || updateMechanism === 'script' ?
                    <SpinnerButton
                      className={styles.updateAvailable}
                      kind={kinds.PRIMARY}
                      isSpinning={isInstallingUpdate}
                      onPress={onInstallLatestPress}
                    >
                      Install Latest
                    </SpinnerButton> :

                    <Fragment>
                      <Icon
                        name={icons.WARNING}
                        kind={kinds.WARNING}
                        size={30}
                      />

                      <div className={styles.message}>
                        {externalUpdaterMessages[updateMechanism] || externalUpdaterMessages.external}
                      </div>
                    </Fragment>
                }

                {
                  isFetching &&
                    <LoadingIndicator
                      className={styles.loading}
                      size={20}
                    />
                }
              </div>
          }

          {
            noUpdateToInstall &&
              <div className={styles.messageContainer}>
                <Icon
                  className={styles.upToDateIcon}
                  name={icons.CHECK_CIRCLE}
                  size={30}
                />

                <div className={styles.message}>
                  The latest version of Sonarr is already installed
                </div>

                {
                  isFetching &&
                    <LoadingIndicator
                      className={styles.loading}
                      size={20}
                    />
                }
              </div>
          }

          {
            hasUpdates &&
              <div>
                {
                  items.map((update) => {
                    const hasChanges = !!update.changes;

                    return (
                      <div
                        key={update.version}
                        className={styles.update}
                      >
                        <div className={styles.info}>
                          <div className={styles.version}>{update.version}</div>
                          <div className={styles.space}>&mdash;</div>
                          <div className={styles.date}>{formatDate(update.releaseDate, shortDateFormat)}</div>

                          {
                            update.branch === 'master' ?
                              null:
                              <Label
                                className={styles.label}
                              >
                                {update.branch}
                              </Label>
                          }

                          {
                            update.version === currentVersion ?
                              <Label
                                className={styles.label}
                                kind={kinds.SUCCESS}
                              >
                                Currently Installed
                              </Label> :
                              null
                          }
                        </div>

                        {
                          !hasChanges &&
                            <div>Maintenance release</div>
                        }

                        {
                          hasChanges &&
                            <div className={styles.changes}>
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
                    );
                  })
                }
              </div>
          }

          {
            !!updatesError &&
              <div>
                Failed to fetch updates
              </div>
          }

          {
            !!generalSettingsError &&
              <div>
                Failed to update settings
              </div>
          }
        </PageContentBodyConnector>
      </PageContent>
    );
  }

}

Updates.propTypes = {
  currentVersion: PropTypes.string.isRequired,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  updatesError: PropTypes.object,
  generalSettingsError: PropTypes.object,
  items: PropTypes.array.isRequired,
  isInstallingUpdate: PropTypes.bool.isRequired,
  updateMechanism: PropTypes.string,
  shortDateFormat: PropTypes.string.isRequired,
  onInstallLatestPress: PropTypes.func.isRequired
};

export default Updates;
