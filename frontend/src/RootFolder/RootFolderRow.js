import PropTypes from 'prop-types';
import React from 'react';
import formatBytes from 'Utilities/Number/formatBytes';
import { icons, kinds } from 'Helpers/Props';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import TableRow from 'Components/Table/TableRow';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import styles from './RootFolderRow.css';

function RootFolderRow(props) {
  const {
    id,
    path,
    freeSpace,
    unmappedFolders,
    onDeletePress
  } = props;

  const unmappedFoldersCount = unmappedFolders.length || '-';
  const isUnavailable = freeSpace == null;

  return (
    <TableRow>
      <TableRowCell>
        {
          isUnavailable ?
            <div className={styles.unavailablePath}>
              {path}

              <Label
                className={styles.unavailableLabel}
                kind={kinds.DANGER}
              >
                Unavailable
              </Label>
            </div> :

            <Link
              className={styles.link}
              to={`/add/import/${id}`}
            >
              {path}
            </Link>
        }
      </TableRowCell>

      <TableRowCell className={styles.freeSpace}>
        {freeSpace ? formatBytes(freeSpace) : '-'}
      </TableRowCell>

      <TableRowCell className={styles.unmappedFolders}>
        {unmappedFoldersCount}
      </TableRowCell>

      <TableRowCell className={styles.actions}>
        <IconButton
          title="Remove root folder"
          name={icons.REMOVE}
          onPress={onDeletePress}
        />
      </TableRowCell>
    </TableRow>
  );
}

RootFolderRow.propTypes = {
  id: PropTypes.number.isRequired,
  path: PropTypes.string.isRequired,
  freeSpace: PropTypes.number,
  unmappedFolders: PropTypes.arrayOf(PropTypes.object).isRequired,
  onDeletePress: PropTypes.func.isRequired
};

RootFolderRow.defaultProps = {
  unmappedFolders: []
};

export default RootFolderRow;
