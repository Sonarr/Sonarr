import React, { useCallback } from 'react';
import Link from 'Components/Link/Link';
import translate from 'Utilities/String/translate';
import styles from './SelectDownloadClientRow.css';

interface SelectSeasonRowProps {
  id: number;
  name: string;
  priority: number;
  onDownloadClientSelect(downloadClientId: number): unknown;
}

function SelectDownloadClientRow(props: SelectSeasonRowProps) {
  const { id, name, priority, onDownloadClientSelect } = props;

  const onSeasonSelectWrapper = useCallback(() => {
    onDownloadClientSelect(id);
  }, [id, onDownloadClientSelect]);

  return (
    <Link
      className={styles.downloadClient}
      component="div"
      onPress={onSeasonSelectWrapper}
    >
      <div>{name}</div>
      <div>{translate('PrioritySettings', { priority })}</div>
    </Link>
  );
}

export default SelectDownloadClientRow;
