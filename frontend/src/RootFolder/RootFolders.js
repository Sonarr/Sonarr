import PropTypes from 'prop-types';
import React from 'react';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import RootFolderRowConnector from './RootFolderRowConnector';

const rootFolderColumns = [
  {
    name: 'path',
    label: 'Path',
    isVisible: true
  },
  {
    name: 'freeSpace',
    label: 'Free Space',
    isVisible: true
  },
  {
    name: 'unmappedFolders',
    label: 'Unmapped Folders',
    isVisible: true
  },
  {
    name: 'actions',
    isVisible: true
  }
];

function RootFolders(props) {
  const {
    isFetching,
    isPopulated,
    error,
    items
  } = props;

  if (isFetching && !isPopulated) {
    return (
      <LoadingIndicator />
    );
  }

  if (!isFetching && !!error) {
    return (
      <div>Unable to load root folders</div>
    );
  }

  return (
    <Table
      columns={rootFolderColumns}
    >
      <TableBody>
        {
          items.map((rootFolder) => {
            return (
              <RootFolderRowConnector
                key={rootFolder.id}
                id={rootFolder.id}
                path={rootFolder.path}
                accessible={rootFolder.accessible}
                freeSpace={rootFolder.freeSpace}
                unmappedFolders={rootFolder.unmappedFolders}
              />
            );
          })
        }
      </TableBody>
    </Table>
  );
}

RootFolders.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired
};

export default RootFolders;
