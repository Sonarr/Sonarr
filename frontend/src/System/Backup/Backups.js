import PropTypes from 'prop-types';
import React, { Component } from 'react';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { icons } from 'Helpers/Props';
import BackupRow from './BackupRow';
import RestoreBackupModalConnector from './RestoreBackupModalConnector';

const columns = [
  {
    name: 'type',
    isVisible: true
  },
  {
    name: 'name',
    label: 'Name',
    isVisible: true
  },
  {
    name: 'size',
    label: 'Size',
    isVisible: true
  },
  {
    name: 'time',
    label: 'Time',
    isVisible: true
  },
  {
    name: 'actions',
    isVisible: true
  }
];

class Backups extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isRestoreModalOpen: false
    };
  }

  //
  // Listeners

  onRestorePress = () => {
    this.setState({ isRestoreModalOpen: true });
  };

  onRestoreModalClose = () => {
    this.setState({ isRestoreModalOpen: false });
  };

  //
  // Render

  render() {
    const {
      isFetching,
      isPopulated,
      error,
      items,
      backupExecuting,
      onBackupPress,
      onDeleteBackupPress
    } = this.props;

    const hasBackups = isPopulated && !!items.length;
    const noBackups = isPopulated && !items.length;

    return (
      <PageContent title="Backups">
        <PageToolbar>
          <PageToolbarSection>
            <PageToolbarButton
              label="Backup Now"
              iconName={icons.BACKUP}
              isSpinning={backupExecuting}
              onPress={onBackupPress}
            />

            <PageToolbarButton
              label="Restore Backup"
              iconName={icons.RESTORE}
              onPress={this.onRestorePress}
            />
          </PageToolbarSection>
        </PageToolbar>

        <PageContentBody>
          {
            isFetching && !isPopulated &&
              <LoadingIndicator />
          }

          {
            !isFetching && !!error &&
              <div>Unable to load backups</div>
          }

          {
            noBackups &&
              <div>No backups are available</div>
          }

          {
            hasBackups &&
              <Table
                columns={columns}
              >
                <TableBody>
                  {
                    items.map((item) => {
                      const {
                        id,
                        type,
                        name,
                        path,
                        size,
                        time
                      } = item;

                      return (
                        <BackupRow
                          key={id}
                          id={id}
                          type={type}
                          name={name}
                          path={path}
                          size={size}
                          time={time}
                          onDeleteBackupPress={onDeleteBackupPress}
                        />
                      );
                    })
                  }
                </TableBody>
              </Table>
          }
        </PageContentBody>

        <RestoreBackupModalConnector
          isOpen={this.state.isRestoreModalOpen}
          onModalClose={this.onRestoreModalClose}
        />
      </PageContent>
    );
  }

}

Backups.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.array.isRequired,
  backupExecuting: PropTypes.bool.isRequired,
  onBackupPress: PropTypes.func.isRequired,
  onDeleteBackupPress: PropTypes.func.isRequired
};

export default Backups;
