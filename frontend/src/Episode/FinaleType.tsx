import React, { useMemo } from 'react';
import Label from 'Components/Label';
import { kinds } from 'Helpers/Props';
import getFinaleTypeName from './getFinaleTypeName';
import styles from './FinaleType.css';

interface SeriesStatusCellProps {
  finaleType: string;
}

function FinaleType(props: SeriesStatusCellProps) {
  const { finaleType } = props;

  const finaleText = useMemo(() => {
    return getFinaleTypeName(finaleType);
  }, [finaleType]);

  if (finaleType == null || finaleText == null) {
    return null;
  }

  return (
    <Label className={styles.label} kind={kinds.INFO}>
      {finaleText}
    </Label>
  );
}

export default FinaleType;
