import PropTypes from 'prop-types';
import React, { Component } from 'react';
import TableRowButton from 'Components/Table/TableRowButton';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';

class SelectEpisodeRow extends Component {

  //
  // Listeners

  onPress = () => {
    const {
      id,
      isSelected
    } = this.props;

    this.props.onSelectedChange({ id, value: !isSelected });
  }

  //
  // Render

  render() {
    const {
      id,
      episodeNumber,
      title,
      airDate,
      isSelected,
      onSelectedChange
    } = this.props;

    return (
      <TableRowButton onPress={this.onPress}>
        <TableSelectCell
          id={id}
          isSelected={isSelected}
          onSelectedChange={onSelectedChange}
        />

        <TableRowCell>
          {episodeNumber}
        </TableRowCell>

        <TableRowCell>
          {title}
        </TableRowCell>

        <TableRowCell>
          {airDate}
        </TableRowCell>
      </TableRowButton>
    );
  }
}

SelectEpisodeRow.propTypes = {
  id: PropTypes.number.isRequired,
  episodeNumber: PropTypes.number.isRequired,
  title: PropTypes.string.isRequired,
  airDate: PropTypes.string.isRequired,
  isSelected: PropTypes.bool,
  onSelectedChange: PropTypes.func.isRequired
};

export default SelectEpisodeRow;
