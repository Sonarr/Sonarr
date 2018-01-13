import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons } from 'Helpers/Props';
import Icon from 'Components/Icon';
import TableRowButton from 'Components/Table/TableRowButton';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import styles from './FileBrowserRow.css';

function getIconName(type) {
  switch (type) {
    case 'computer':
      return icons.COMPUTER;
    case 'drive':
      return icons.DRIVE;
    case 'file':
      return icons.FILE;
    case 'parent':
      return icons.PARENT;
    default:
      return icons.FOLDER;
  }
}

class FileBrowserRow extends Component {

  //
  // Listeners

  onPress = () => {
    this.props.onPress(this.props.path);
  }

  //
  // Render

  render() {
    const {
      type,
      name
    } = this.props;

    return (
      <TableRowButton onPress={this.onPress}>
        <TableRowCell className={styles.type}>
          <Icon name={getIconName(type)} />
        </TableRowCell>

        <TableRowCell>{name}</TableRowCell>
      </TableRowButton>
    );
  }

}

FileBrowserRow.propTypes = {
  type: PropTypes.string.isRequired,
  name: PropTypes.string.isRequired,
  path: PropTypes.string.isRequired,
  onPress: PropTypes.func.isRequired
};

export default FileBrowserRow;
