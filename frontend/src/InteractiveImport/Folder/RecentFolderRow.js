import PropTypes from 'prop-types';
import React, { Component } from 'react';
import TableRowButton from 'Components/Table/TableRowButton';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';

class RecentFolderRow extends Component {

  //
  // Listeners

  onPress = () => {
    this.props.onPress(this.props.folder);
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
      </TableRowButton>
    );
  }
}

RecentFolderRow.propTypes = {
  folder: PropTypes.string.isRequired,
  lastUsed: PropTypes.string.isRequired,
  onPress: PropTypes.func.isRequired
};

export default RecentFolderRow;
