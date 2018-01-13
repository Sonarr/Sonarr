import PropTypes from 'prop-types';
import React from 'react';
import formatBytes from 'Utilities/Number/formatBytes';
import { icons } from 'Helpers/Props';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import TableRow from 'Components/Table/TableRow';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import styles from './ImportSeriesRootFolderRow.css';

function ImportSeriesRootFolderRow(props) {
  const {
    id,
    path,
    freeSpace,
    unmappedFolders,
    onDeletePress
  } = props;

  const unmappedFoldersCount = unmappedFolders.length || '-';

  return (
    <TableRow>
      <TableRowCell>
        <Link
          className={styles.link}
          to={`/add/import/${id}`}
        >
          {path}
        </Link>
      </TableRowCell>

      <TableRowCell className={styles.freeSpace}>
        {formatBytes(freeSpace) || '-'}
      </TableRowCell>

      <TableRowCell className={styles.unmappedFolders}>
        {unmappedFoldersCount}
      </TableRowCell>

      <TableRowCell className={styles.actions}>
        <IconButton
          name={icons.REMOVE}
          onPress={onDeletePress}
        />
      </TableRowCell>
    </TableRow>
  );
}

ImportSeriesRootFolderRow.propTypes = {
  id: PropTypes.number.isRequired,
  path: PropTypes.string.isRequired,
  freeSpace: PropTypes.number.isRequired,
  unmappedFolders: PropTypes.arrayOf(PropTypes.object).isRequired,
  onDeletePress: PropTypes.func.isRequired
};

ImportSeriesRootFolderRow.defaultProps = {
  freeSpace: 0,
  unmappedFolders: []
};

export default ImportSeriesRootFolderRow;
