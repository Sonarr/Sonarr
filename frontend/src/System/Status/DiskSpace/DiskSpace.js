import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { kinds, sizes } from 'Helpers/Props';
import formatBytes from 'Utilities/Number/formatBytes';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import FieldSet from 'Components/FieldSet';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import TableRow from 'Components/Table/TableRow';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import ProgressBar from 'Components/ProgressBar';
import styles from './DiskSpace.css';

const columns = [
  {
    name: 'path',
    label: 'Location',
    isVisible: true
  },
  {
    name: 'freeSpace',
    label: 'Free Space',
    isVisible: true
  },
  {
    name: 'totalSpace',
    label: 'Total Space',
    isVisible: true
  },
  {
    name: 'progress',
    isVisible: true
  }
];

class DiskSpace extends Component {

  //
  // Render

  render() {
    const {
      isFetching,
      items
    } = this.props;

    return (
      <FieldSet legend="Disk Space">
        {
          isFetching &&
            <LoadingIndicator />
        }

        {
          !isFetching &&
            <Table
              columns={columns}
            >
              <TableBody>
                {
                  items.map((item) => {
                    const {
                      freeSpace,
                      totalSpace
                    } = item;

                    const diskUsage = (100 - freeSpace / totalSpace * 100);
                    let diskUsageKind = kinds.PRIMARY;

                    if (diskUsage > 90) {
                      diskUsageKind = kinds.DANGER;
                    } else if (diskUsage > 80) {
                      diskUsageKind = kinds.WARNING;
                    }

                    return (
                      <TableRow key={item.path}>
                        <TableRowCell>
                          {item.path}

                          {
                            item.label &&
                              ` (${item.label})`
                          }
                        </TableRowCell>

                        <TableRowCell className={styles.space}>
                          {formatBytes(freeSpace)}
                        </TableRowCell>

                        <TableRowCell className={styles.space}>
                          {formatBytes(totalSpace)}
                        </TableRowCell>

                        <TableRowCell className={styles.space}>
                          <ProgressBar
                            progress={diskUsage}
                            kind={diskUsageKind}
                            size={sizes.MEDIUM}
                          />
                        </TableRowCell>
                      </TableRow>
                    );
                  })
                }
              </TableBody>
            </Table>
        }
      </FieldSet>
    );
  }

}

DiskSpace.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  items: PropTypes.array.isRequired
};

export default DiskSpace;
