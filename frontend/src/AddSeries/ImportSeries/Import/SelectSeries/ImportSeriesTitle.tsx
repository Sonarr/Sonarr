import React from 'react';
import Label from 'Components/Label';
import { kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './ImportSeriesTitle.css';

interface ImportSeriesTitleProps {
  title: string;
  year: number;
  network?: string;
  isExistingSeries: boolean;
}

function ImportSeriesTitle({
  title,
  year,
  network,
  isExistingSeries,
}: ImportSeriesTitleProps) {
  return (
    <div className={styles.titleContainer}>
      <div className={styles.title}>{title}</div>

      {year > 0 && !title.includes(String(year)) ? (
        <span className={styles.year}>({year})</span>
      ) : null}

      {network ? <Label>{network}</Label> : null}

      {isExistingSeries ? (
        <Label kind={kinds.WARNING}>{translate('Existing')}</Label>
      ) : null}
    </div>
  );
}

export default ImportSeriesTitle;
