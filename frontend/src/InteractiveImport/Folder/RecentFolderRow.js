import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons } from 'Helpers/Props';
import IconButton from 'Components/Link/IconButton';
import TableRowButton from 'Components/Table/TableRowButton';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import styles from './RecentFolderRow.css';

class RecentFolderRow extends Component {

  //
  // Listeners

  onPress = () => {
    this.props.onPress(this.props.folder);
  }

  onRemovePress = (event) => {
    event.stopPropagation();

    const {
      folder,
      onRemoveRecentFolderPress
    } = this.props;

    onRemoveRecentFolderPress(folder);
  }

  //
  // Render

  render() {
    const {
      folder,
      lastUsed
    } = this.props;

    return (
      <TableRowButton onPress={this.onPress}>
        <TableRowCell>{folder}</TableRowCell>

        <RelativeDateCellConnector date={lastUsed} />

        <TableRowCell className={styles.actions}>
          <IconButton
            name={icons.REMOVE}
            title="Remove"
            onPress={this.onRemovePress}
          />
        </TableRowCell>
      </TableRowButton>
    );
  }
}

RecentFolderRow.propTypes = {
  folder: PropTypes.string.isRequired,
  lastUsed: PropTypes.string.isRequired,
  onPress: PropTypes.func.isRequired,
  onRemoveRecentFolderPress: PropTypes.func.isRequired
};

export default RecentFolderRow;
