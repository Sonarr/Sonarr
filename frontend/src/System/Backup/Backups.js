import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons } from 'Helpers/Props';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import TableRow from 'Components/Table/TableRow';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import styles from './Backups.css';

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
  }
];

class Backups extends Component {

  //
  // Render

  render() {
    const {
      isFetching,
      items,
      backupExecuting,
      onBackupPress
    } = this.props;

    const hasBackups = !isFetching && items.length > 0;
    const noBackups = !isFetching && !items.length;

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
          </PageToolbarSection>
        </PageToolbar>

        <PageContentBodyConnector>
          {
            isFetching &&
              <LoadingIndicator />
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

                      let iconClassName = icons.SCHEDULED;
                      let iconTooltip = 'Scheduled';

                      if (type === 'manual') {
                        iconClassName = icons.INTERACTIVE;
                        iconTooltip = 'Manual';
                      } else if (item === 'update') {
                        iconClassName = icons.UPDATE;
                        iconTooltip = 'Before update';
                      }

                      return (
                        <TableRow key={id}>
                          <TableRowCell className={styles.type}>
                            {
                              <Icon
                                name={iconClassName}
                                title={iconTooltip}
                              />
                            }
                          </TableRowCell>

                          <TableRowCell>
                            <Link
                              to={path}
                              noRouter={true}
                            >
                              {name}
                            </Link>
                          </TableRowCell>

                          <RelativeDateCellConnector
                            className={styles.time}
                            date={time}
                          />
                        </TableRow>
                      );
                    })
                  }
                </TableBody>
              </Table>
          }
        </PageContentBodyConnector>
      </PageContent>
    );
  }

}

Backups.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  items: PropTypes.array.isRequired,
  backupExecuting: PropTypes.bool.isRequired,
  onBackupPress: PropTypes.func.isRequired
};

export default Backups;
