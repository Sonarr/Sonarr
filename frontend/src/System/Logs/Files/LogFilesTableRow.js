import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Link from 'Components/Link/Link';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import styles from './LogFilesTableRow.css';

class LogFilesTableRow extends Component {

  //
  // Render

  render() {
    const {
      filename,
      lastWriteTime,
      downloadUrl
    } = this.props;

    return (
      <TableRow>
        <TableRowCell>{filename}</TableRowCell>

        <RelativeDateCellConnector
          date={lastWriteTime}
        />

        <TableRowCell className={styles.download}>
          <Link
            to={downloadUrl}
            target="_blank"
            noRouter={true}
          >
            Download
          </Link>
        </TableRowCell>
      </TableRow>
    );
  }

}

LogFilesTableRow.propTypes = {
  filename: PropTypes.string.isRequired,
  lastWriteTime: PropTypes.string.isRequired,
  downloadUrl: PropTypes.string.isRequired
};

export default LogFilesTableRow;
