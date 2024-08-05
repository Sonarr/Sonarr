import React, { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import FieldSet from 'Components/FieldSet';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ProgressBar from 'Components/ProgressBar';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import Column from 'Components/Table/Column';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import TableRow from 'Components/Table/TableRow';
import { kinds, sizes } from 'Helpers/Props';
import { fetchDiskSpace } from 'Store/Actions/systemActions';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import styles from './DiskSpace.css';

const columns: Column[] = [
  {
    name: 'path',
    label: () => translate('Location'),
    isVisible: true,
  },
  {
    name: 'freeSpace',
    label: () => translate('FreeSpace'),
    isVisible: true,
  },
  {
    name: 'totalSpace',
    label: () => translate('TotalSpace'),
    isVisible: true,
  },
  {
    name: 'progress',
    label: '',
    isVisible: true,
  },
];

function createDiskSpaceSelector() {
  return createSelector(
    (state: AppState) => state.system.diskSpace,
    (diskSpace) => {
      return diskSpace;
    }
  );
}

function DiskSpace() {
  const dispatch = useDispatch();
  const { isFetching, items } = useSelector(createDiskSpaceSelector());

  useEffect(() => {
    dispatch(fetchDiskSpace());
  }, [dispatch]);

  return (
    <FieldSet legend={translate('DiskSpace')}>
      {isFetching ? <LoadingIndicator /> : null}

      {isFetching ? null : (
        <Table columns={columns}>
          <TableBody>
            {items.map((item) => {
              const { freeSpace, totalSpace } = item;

              const diskUsage = 100 - (freeSpace / totalSpace) * 100;
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

                    {item.label && ` (${item.label})`}
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
            })}
          </TableBody>
        </Table>
      )}
    </FieldSet>
  );
}

export default DiskSpace;
