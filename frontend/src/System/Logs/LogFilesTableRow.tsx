import React from 'react';
import Link from 'Components/Link/Link';
import RelativeDateCell from 'Components/Table/Cells/RelativeDateCell';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import translate from 'Utilities/String/translate';
import styles from './LogFilesTableRow.css';

interface LogFilesTableRowProps {
  filename: string;
  lastWriteTime: string;
  downloadUrl: string;
}

function LogFilesTableRow({
  filename,
  lastWriteTime,
  downloadUrl,
}: LogFilesTableRowProps) {
  return (
    <TableRow>
      <TableRowCell>{filename}</TableRowCell>

      <RelativeDateCell date={lastWriteTime} />

      <TableRowCell className={styles.download}>
        <Link to={downloadUrl} target="_blank" noRouter={true}>
          {translate('Download')}
        </Link>
      </TableRowCell>
    </TableRow>
  );
}

export default LogFilesTableRow;
