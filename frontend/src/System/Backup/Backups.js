import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons } from 'Helpers/Props';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
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
  }

  onRestoreModalClose = () => {
    this.setState({ isRestoreModalOpen: false });
  }

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

        <PageContentBodyConnector>
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
                        time
                      } = item;

                      return (
                        <BackupRow
                          key={id}
                          id={id}
                          type={type}
                          name={name}
                          path={path}
                          time={time}
                          onDeleteBackupPress={onDeleteBackupPress}
                        />
                      );
                    })
                  }
                </TableBody>
              </Table>
          }
        </PageContentBodyConnector>

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
