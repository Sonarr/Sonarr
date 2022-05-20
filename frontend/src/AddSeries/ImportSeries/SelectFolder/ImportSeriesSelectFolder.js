import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import FileBrowserModal from 'Components/FileBrowser/FileBrowserModal';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import { icons, kinds, sizes } from 'Helpers/Props';
import RootFolders from 'RootFolder/RootFolders';
import styles from './ImportSeriesSelectFolder.css';

class ImportSeriesSelectFolder extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isAddNewRootFolderModalOpen: false
    };
  }

  //
  // Lifecycle

  onAddNewRootFolderPress = () => {
    this.setState({ isAddNewRootFolderModalOpen: true });
  };

  onNewRootFolderSelect = ({ value }) => {
    this.props.onNewRootFolderSelect(value);
  };

  onAddRootFolderModalClose = () => {
    this.setState({ isAddNewRootFolderModalOpen: false });
  };

  //
  // Render

  render() {
    const {
      isWindows,
      isFetching,
      isPopulated,
      isSaving,
      error,
      saveError,
      items
    } = this.props;

    const hasRootFolders = items.length > 0;

    return (
      <PageContent title="Import Series">
        <PageContentBody>
          {
            isFetching && !isPopulated ?
              <LoadingIndicator /> :
              null
          }

          {
            !isFetching && error ?
              <div>Unable to load root folders</div> :
              null
          }

          {
            !error && isPopulated &&
              <div>
                <div className={styles.header}>
                  Import series you already have
                </div>

                <div className={styles.tips}>
                  Some tips to ensure the import goes smoothly:
                  <ul>
                    <li className={styles.tip}>
                      Make sure that your files include the quality in their filenames. eg. <span className={styles.code}>episode.s02e15.bluray.mkv</span>
                    </li>
                    <li className={styles.tip}>
                      Point Sonarr to the folder containing all of your tv shows, not a specific one. eg. <span className={styles.code}>"{isWindows ? 'C:\\tv shows' : '/tv shows'}"</span> and not <span className={styles.code}>"{isWindows ? 'C:\\tv shows\\the simpsons' : '/tv shows/the simpsons'}"</span> Additionally, each series must be in its own folder within the root/library folder.
                    </li>
                    <li className={styles.tip}>
                      Do not use for importing downloads from your download client, this is only for existing organized libraries, not unsorted files.
                    </li>
                  </ul>
                </div>

                {
                  hasRootFolders ?
                    <div className={styles.recentFolders}>
                      <FieldSet legend="Root Folders">
                        <RootFolders
                          isFetching={isFetching}
                          isPopulated={isPopulated}
                          error={error}
                          items={items}
                        />
                      </FieldSet>
                    </div> :
                    null
                }

                {
                  !isSaving && saveError ?
                    <Alert
                      className={styles.addErrorAlert}
                      kind={kinds.DANGER}
                    >
                      Unable to add root folder

                      <ul>
                        {
                          saveError.responseJSON.map((e, index) => {
                            return (
                              <li key={index}>
                                {e.errorMessage}
                              </li>
                            );
                          })
                        }
                      </ul>
                    </Alert> :
                    null
                }

                <div className={hasRootFolders ? undefined : styles.startImport}>
                  <Button
                    kind={kinds.PRIMARY}
                    size={sizes.LARGE}
                    onPress={this.onAddNewRootFolderPress}
                  >
                    <Icon
                      className={styles.importButtonIcon}
                      name={icons.DRIVE}
                    />
                    {
                      hasRootFolders ?
                        'Choose another folder' :
                        'Start Import'
                    }
                  </Button>
                </div>

                <FileBrowserModal
                  isOpen={this.state.isAddNewRootFolderModalOpen}
                  name="rootFolderPath"
                  value=""
                  onChange={this.onNewRootFolderSelect}
                  onModalClose={this.onAddRootFolderModalClose}
                />
              </div>
          }
        </PageContentBody>
      </PageContent>
    );
  }
}

ImportSeriesSelectFolder.propTypes = {
  isWindows: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  isSaving: PropTypes.bool.isRequired,
  error: PropTypes.object,
  saveError: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  onNewRootFolderSelect: PropTypes.func.isRequired
};

export default ImportSeriesSelectFolder;
