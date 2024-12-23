import React from 'react';
import Label from 'Components/Label';
import VirtualTableRowCell from 'Components/Table/Cells/VirtualTableRowCell';
import styles from './SelectSeriesRow.css';

interface SelectSeriesRowProps {
  title: string;
  tvdbId: number;
  imdbId?: string;
  year: number;
}

function SelectSeriesRow({
  title,
  year,
  tvdbId,
  imdbId,
}: SelectSeriesRowProps) {
  return (
    <>
      <VirtualTableRowCell className={styles.title}>
        {title}
      </VirtualTableRowCell>

      <VirtualTableRowCell className={styles.year}>{year}</VirtualTableRowCell>

      <VirtualTableRowCell className={styles.tvdbId}>
        <Label>{tvdbId}</Label>
      </VirtualTableRowCell>

      <VirtualTableRowCell className={styles.imdbId}>
        {imdbId ? <Label>{imdbId}</Label> : null}
      </VirtualTableRowCell>
    </>
  );
}

export default SelectSeriesRow;
