import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import FileBrowserModal from 'Components/FileBrowser/FileBrowserModal';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import InlineMarkdown from 'Components/Markdown/InlineMarkdown';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import { icons, kinds, sizes } from 'Helpers/Props';
import RootFolders from 'RootFolder/RootFolders';
import translate from 'Utilities/String/translate';
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
    const goodFolderExample = (isWindows) ? 'C:\\tv shows' : '/tv shows';
    const badFolderExample = (isWindows) ? 'C:\\tv shows\\the simpsons' : '/tv shows/the simpsons';

    return (
      <PageContent title={translate('ImportSeries')}>
        <PageContentBody>
          {
            isFetching && !isPopulated ?
              <LoadingIndicator /> :
              null
          }

          {
            !isFetching && error ?
              <Alert kind={kinds.DANGER}>{translate('RootFoldersLoadError')}</Alert> :
              null
          }

          {
            !error && isPopulated &&
              <div>
                <div className={styles.header}>
                  {translate('LibraryImportSeriesHeader')}
                </div>

                <div className={styles.tips}>
                  {translate('LibraryImportTips')}
                  <ul>
                    <li className={styles.tip}>
                      <InlineMarkdown data={translate('LibraryImportTipsQualityInEpisodeFilename')} />
                    </li>
                    <li className={styles.tip}>
                      <InlineMarkdown data={translate('LibraryImportTipsSeriesUseRootFolder', { goodFolderExample, badFolderExample })} />
                    </li>
                    <li className={styles.tip}>
                      {translate('LibraryImportTipsDontUseDownloadsFolder')}
                    </li>
                  </ul>
                </div>

                {
                  hasRootFolders ?
                    <div className={styles.recentFolders}>
                      <FieldSet legend={translate('RootFolders')}>
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
                      {translate('AddRootFolderError')}

                      <ul>
                        {
                          Array.isArray(saveError.responseJSON) ?
                            saveError.responseJSON.map((e, index) => {
                              return (
                                <li key={index}>
                                  {e.errorMessage}
                                </li>
                              );
                            }) :
                            <li>
                              {
                                JSON.stringify(saveError.responseJSON)
                              }
                            </li>
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
                        translate('ChooseAnotherFolder') :
                        translate('StartImport')
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
