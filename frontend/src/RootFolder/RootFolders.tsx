import React from 'react';
import Alert from 'Components/Alert';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Column from 'Components/Table/Column';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import RootFolderRow from './RootFolderRow';
import useRootFolders from './useRootFolders';

const rootFolderColumns: Column[] = [
  {
    name: 'path',
    label: () => translate('Path'),
    isVisible: true,
  },
  {
    name: 'freeSpace',
    label: () => translate('FreeSpace'),
    isVisible: true,
  },
  {
    name: 'unmappedFolders',
    label: () => translate('UnmappedFolders'),
    isVisible: true,
  },
  {
    name: 'actions',
    label: '',
    isVisible: true,
  },
];

function RootFolders() {
  const { isFetching, isFetched, error, data } = useRootFolders();

  if (isFetching && !isFetched) {
    return <LoadingIndicator />;
  }

  if (!isFetching && !!error) {
    return (
      <Alert kind={kinds.DANGER}>{translate('RootFoldersLoadError')}</Alert>
    );
  }

  return (
    <Table columns={rootFolderColumns}>
      <TableBody>
        {data.map((rootFolder) => {
          return (
            <RootFolderRow
              key={rootFolder.id}
              id={rootFolder.id}
              path={rootFolder.path}
              accessible={rootFolder.accessible}
              isEmpty={rootFolder.isEmpty}
              freeSpace={rootFolder.freeSpace}
              unmappedFolders={rootFolder.unmappedFolders}
            />
          );
        })}
      </TableBody>
    </Table>
  );
}

export default RootFolders;
