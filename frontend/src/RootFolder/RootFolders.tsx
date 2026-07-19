import React from 'react';
import Alert from 'Components/Alert';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import { kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import RootFolderRow from './RootFolderRow';
import useRootFolders from './useRootFolders';
import styles from './RootFolders.css';

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
    <div className={styles.list}>
      {data.map((rootFolder) => (
        <RootFolderRow key={rootFolder.id} {...rootFolder} />
      ))}
    </div>
  );
}

export default RootFolders;
