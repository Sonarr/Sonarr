import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons, kinds, sizes } from 'Helpers/Props';
import Button from 'Components/Link/Button';
import Icon from 'Components/Icon';
import PathInputConnector from 'Components/Form/PathInputConnector';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import RecentFolderRow from './RecentFolderRow';
import styles from './InteractiveImportSelectFolderModalContent.css';

const recentFoldersColumns = [
  {
    name: 'folder',
    label: 'Folder'
  },
  {
    name: 'lastUsed',
    label: 'Last Used'
  },
  {
    name: 'actions',
    label: ''
  }
];

class InteractiveImportSelectFolderModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      folder: ''
    };
  }

  //
  // Listeners

  onPathChange = ({ value }) => {
    this.setState({ folder: value });
  }

  onRecentPathPress = (folder) => {
    this.setState({ folder });
  }

  onQuickImportPress = () => {
    this.props.onQuickImportPress(this.state.folder);
  }

  onInteractiveImportPress = () => {
    this.props.onInteractiveImportPress(this.state.folder);
  }

  //
  // Render

  render() {
    const {
      recentFolders,
      onRemoveRecentFolderPress,
      onModalClose
    } = this.props;

    const folder = this.state.folder;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          Manual Import - Select Folder
        </ModalHeader>

        <ModalBody>
          <PathInputConnector
            name="folder"
            value={folder}
            onChange={this.onPathChange}
          />

          {
            !!recentFolders.length &&
              <div className={styles.recentFoldersContainer}>
                <Table
                  columns={recentFoldersColumns}
                >
                  <TableBody>
                    {
                      recentFolders.map((recentFolder) => {
                        return (
                          <RecentFolderRow
                            key={recentFolder.folder}
                            folder={recentFolder.folder}
                            lastUsed={recentFolder.lastUsed}
                            onPress={this.onRecentPathPress}
                            onRemoveRecentFolderPress={onRemoveRecentFolderPress}
                          />
                        );
                      })
                    }
                  </TableBody>
                </Table>
              </div>
          }

          <div className={styles.buttonsContainer}>
            <div className={styles.buttonContainer}>
              <Button
                className={styles.button}
                kind={kinds.PRIMARY}
                size={sizes.LARGE}
                isDisabled={!folder}
                onPress={this.onQuickImportPress}
              >
                <Icon
                  className={styles.buttonIcon}
                  name={icons.QUICK}
                />

                Quick Import
              </Button>
            </div>

            <div className={styles.buttonContainer}>
              <Button
                className={styles.button}
                kind={kinds.PRIMARY}
                size={sizes.LARGE}
                isDisabled={!folder}
                onPress={this.onInteractiveImportPress}
              >
                <Icon
                  className={styles.buttonIcon}
                  name={icons.INTERACTIVE}
                />

                Interactive Import
              </Button>
            </div>
          </div>
        </ModalBody>

        <ModalFooter>
          <Button onPress={onModalClose}>
            Cancel
          </Button>
        </ModalFooter>
      </ModalContent>
    );
  }
}

InteractiveImportSelectFolderModalContent.propTypes = {
  recentFolders: PropTypes.arrayOf(PropTypes.object).isRequired,
  onQuickImportPress: PropTypes.func.isRequired,
  onInteractiveImportPress: PropTypes.func.isRequired,
  onRemoveRecentFolderPress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default InteractiveImportSelectFolderModalContent;
